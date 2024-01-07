using Centurion.Accounts.App.Analytics.Model;
using Centurion.Accounts.App.Analytics.Services;
using Centurion.Accounts.Authorization;
using Centurion.Accounts.Foundation.Authorization;
using Centurion.Accounts.Foundation.Model;
using Centurion.Accounts.Foundation.Mvc.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using Centurion.Accounts.Core.Analytics.Services;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;
using Centurion.Accounts.Infra.Analytics.Services;

namespace Centurion.Accounts.Analytics.Controllers;

public class AnalyticsController : SecuredDashboardBoundControllerBase
{
  private readonly IAnalyticsProvider _analyticsProvider;
  private readonly IUserSessionService _userSessionService;
  private readonly IDashboardRepository _dashboardRepository;
  private readonly IDiscordInsightsClient _discordInsightsClient;
  private readonly ILogger<AnalyticsController> _logger;
  private readonly IMemoryCache _memoryCache;

  public AnalyticsController(IServiceProvider provider, IAnalyticsProvider analyticsProvider,
    IUserSessionService userSessionService, IDashboardRepository dashboardRepository,
    IDiscordInsightsClient discordInsightsClient, ILogger<AnalyticsController> logger, IMemoryCache memoryCache)
    : base(provider)
  {
    _analyticsProvider = analyticsProvider;
    _userSessionService = userSessionService;
    _dashboardRepository = dashboardRepository;
    _discordInsightsClient = discordInsightsClient;
    _logger = logger;
    _memoryCache = memoryCache;
  }

  [HttpGet("General")]
  [AuthorizePermission(Permissions.AnalyticsGeneralRead)]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<GeneralAnalytics>))]
  public async ValueTask<IActionResult> GetGeneralAsync([FromQuery] GeneralAnalyticsRequest request,
    CancellationToken ct)
  {
    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(CurrentDashboardId)
      .OrThrowForbid();
      
    var key = $"{request.Start}_{request.Offset}_{request.Period}_{CurrentUserId}_{CurrentDashboardId}";
    if (_memoryCache.TryGetValue<GeneralAnalytics>(key, out var analytics))
    {
      return Ok(analytics);
    }

    var r = await _analyticsProvider.GetGeneralAnalyticsAsync(CurrentDashboardId, request, ct);
    if (r.IsFailure)
    {
      return BadRequest(r.Error);
    }

    _memoryCache.Set(key, r.Value, TimeSpan.FromMinutes(1));

    return Ok(r.Value);
  }

  [HttpGet("Discord")]
  [AuthorizePermission(Permissions.AnalyticsDiscordRead)]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<DiscordInsightsAnalytics>))]
  public async ValueTask<IActionResult> GetDiscordAsync([FromQuery] DiscordAnalyticsRequest request,
    CancellationToken ct)
  {
    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(CurrentDashboardId)
      .OrThrowForbid();

    var key = $"{request.StartAt}_{request.EndAt}_{request.Interval}_{CurrentUserId}_{CurrentDashboardId}";
    if (_memoryCache.TryGetValue<DiscordInsightsAnalytics>(key, out var analytics))
    {
      return Ok(analytics);
    }

    var dashboard = await _dashboardRepository.GetByIdAsync(CurrentDashboardId, ct);
    if (dashboard == null)
    {
      return NotFound();
    }

    var creds = dashboard.DiscordConfig;
    if (creds.IsUserCredentialsEmpty())
    {
      return BadRequest();
    }

    try
    {
      var activationTask = _discordInsightsClient.GetActivationAsync(request, creds, ct).AsTask();
      var joinBySourceTask = _discordInsightsClient.GetJoinBySourceAsync(request, creds, ct).AsTask();
      var membershipTask = _discordInsightsClient.GetMembershipAsync(request, creds, ct).AsTask();
      var leaversTask = _discordInsightsClient.GetLeaversAsync(request, creds, ct).AsTask();
      var retentionTask = _discordInsightsClient.GetRetentionAsync(request, creds, ct).AsTask();
      var overviewTask = _discordInsightsClient.GetOverviewAsync(request, creds, ct).AsTask();

      var tasks = new Task[]
      {
        activationTask, joinBySourceTask, membershipTask, leaversTask, retentionTask, overviewTask
      };

      await Task.WhenAll(tasks);
        
      analytics = new DiscordInsightsAnalytics
      {
        Activation = activationTask.Result,
        JoinBySource = joinBySourceTask.Result,
        Membership = membershipTask.Result,
        Leavers = leaversTask.Result,
        Retention = retentionTask.Result,
        Overview = overviewTask.Result
      };

      _memoryCache.Set(key, analytics, TimeSpan.FromMinutes(1));

      return Ok(analytics);
    }
    catch (Exception e)
    {
      _logger.LogError(e, "Can't fetch discord insights");
      return BadRequest();
    }
  }

  [HttpPost("Sessions")]
  [AllowAnonymous]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<Guid>))]
  public async ValueTask<IActionResult> RefreshOrCreateSessionAsync(Guid? sessionId, CancellationToken ct)
  {
    Guid dashboardId;
    long? userId;
    if (User.Identity?.IsAuthenticated == false)
    {
      string referer = Request.Headers[HeaderNames.Referer];
      if (string.IsNullOrEmpty(referer) || !Uri.TryCreate(referer, UriKind.Absolute, out var refererUrl))
      {
        return BadRequest();
      }

      Dashboard? dashboard = await _dashboardRepository.GetByRawLocationAsync(refererUrl.AbsolutePath, ct);
      if (dashboard == null)
      {
        return BadRequest();
      }

      userId = null;
      dashboardId = dashboard.Id;
    }
    else
    {
      userId = CurrentUserId;
      dashboardId = CurrentDashboardId;
    }

    string userAgent = Request.Headers[HeaderNames.UserAgent];
    var ip = HttpContext.Connection.RemoteIpAddress;
    sessionId = await _userSessionService.RefreshOrCreateSessionAsync(dashboardId, sessionId, userAgent, ip, userId,
      ct);

    return Ok(sessionId);
  }
}