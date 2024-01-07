using Centurion.Accounts.Foundation.Mvc.Controllers;
using Centurion.Accounts.Services.Stripe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Centurion.Accounts.Core.Products.Services;
using Centurion.Accounts.Foundation.Authorization;
using Stripe;

namespace Centurion.Accounts.Controllers;

public class StripeWebHooksController : SecuredControllerBase
{
  private const string StripeWebhookEndpoint = nameof(StripeWebhookEndpoint);

  private readonly IDashboardRepository _dashboardRepository;
  private readonly IEnumerable<IStripeWebHookHandler> _webHookHandlers;
  private readonly ILogger<StripeWebHooksController> _logger;

  public StripeWebHooksController(IDashboardRepository dashboardRepository,
    IServiceProvider serviceProvider,
    IEnumerable<IStripeWebHookHandler> webHookHandlers, ILogger<StripeWebHooksController> logger)
    : base(serviceProvider)
  {
    _dashboardRepository = dashboardRepository;
    _webHookHandlers = webHookHandlers;
    _logger = logger;
  }

  [HttpGet("[controller]")]
  public IActionResult GetWebhookEndpoint()
  {
    var currDashboard = User.GetDashboardId();
    if (!currDashboard.HasValue)
    {
      return NotFound();
    }

    if (!User.GetOwnDashboardIds().Contains(currDashboard.Value))
    {
      return NotFound();
    }

    var url = Url.Link(StripeWebhookEndpoint, new {dashboardId = currDashboard});
    return Ok(url!);
  }

  [HttpPost("{dashboardId:guid}/Webhooks", Name = StripeWebhookEndpoint)]
  [AllowAnonymous]
  public async ValueTask<IActionResult> HandleWebhookAsync([FromHeader(Name = "Stripe-Signature")]
    string signature, Guid dashboardId, CancellationToken ct)
  {
    try
    {
      using var reader = new StreamReader(Request.Body);
      var rawPayload = await reader.ReadToEndAsync();
      _logger.LogDebug("Received stripe webhook. Searching for dashboard with Id {DashboardId}", dashboardId);
      var dashboard = await _dashboardRepository.GetByIdAsync(dashboardId, ct);
      if (dashboard == null)
      {
        _logger.LogWarning("Dashboard {DashboardId} not found", dashboardId);
        return NotFound();
      }

      _logger.LogDebug("Constructing event");
      var @event = EventUtility.ConstructEvent(rawPayload, signature, dashboard.StripeConfig.WebHookEndpointSecret);
      _logger.LogDebug("Event {EventType} constructed", @event.Type);

      var handler = _webHookHandlers.FirstOrDefault(_ => _.CanHandle(@event.Type));
      if (handler == null)
      {
        _logger.LogWarning("No handler for event {EventType}", @event.Type);
        return NoContent();
      }

      _logger.LogDebug("Handling event {EventType}", @event.Type);
      var result = await handler.HandleAsync(@event, dashboard, ct);
      if (result.IsFailure)
      {
        _logger.LogError("Can't handle webhook. {Reason}", result.Error);
        return BadRequest(); 
      }

      _logger.LogDebug("Event {EventType} handled successfully", @event.Type);
      return NoContent();
    }
    catch (StripeException exc)
    {
      _logger.LogError(exc, "Can't handle stripe webhook for dashboard {DashboardId}", dashboardId);
      return BadRequest();
    }
  }
}