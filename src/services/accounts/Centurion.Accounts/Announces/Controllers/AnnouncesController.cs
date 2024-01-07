using Centurion.Accounts.Announces.Hubs;
using Centurion.Accounts.Foundation.Mvc.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Centurion.Accounts.Announces.Controllers;

public class AnnouncesController : SecuredDashboardBoundControllerBase
{
  private const string ApiKeyHeaderName = "X-Api-Key";
  private readonly IHubContext<AnnouncesHub, IAnnouncesHubClient> _hubContext;
  private readonly IConfiguration _config;

  public AnnouncesController(IServiceProvider provider, IHubContext<AnnouncesHub, IAnnouncesHubClient> hubContext,
    IConfiguration config)
    : base(provider)
  {
    _hubContext = hubContext;
    _config = config;
  }

  [HttpPost("broadcast")]
  [AllowAnonymous]
  // [AuthorizePermission(Permissions.AnnouncesBroadcast)]
  public async ValueTask<IActionResult> Broadcast(string title, string message)
  {
    var permittedKeys = _config.GetSection("AnnouncesApi:AuthorizedKeys").Get<string[]>();
    if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var value) && !permittedKeys.Contains(value.ToString()))
    {
      return Forbid();
    }

    // await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(CurrentDashboardId)
    //   .OrThrowForbid();

    await _hubContext.Clients.All.ReceiveAnnounce(title, message);
    return NoContent();
  }
}