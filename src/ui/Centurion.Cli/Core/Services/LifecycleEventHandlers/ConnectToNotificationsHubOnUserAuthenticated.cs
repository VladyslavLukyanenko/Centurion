using Centurion.Cli.Core.Domain.LifecycleEvents;
using Centurion.Cli.Core.Services.LifecycleEvents;
using MediatR;

namespace Centurion.Cli.Core.Services.LifecycleEventHandlers;

public class ConnectToNotificationsHubOnUserAuthenticated
  : IApplicationLifecycleEventHandler<UserAuthenticated>, IApplicationLifecycleEventHandler<UserLoggedOut>
{
  private readonly IApplicationSessionController _controller;

  public ConnectToNotificationsHubOnUserAuthenticated(IApplicationSessionController controller)
  {
    _controller = controller;
  }

  public async Task<Unit> Handle(UserAuthenticated request, CancellationToken ct)
  {
    await HandleAsync(request, ct);
    return Unit.Value;
  }

  public async Task<Unit> Handle(UserLoggedOut request, CancellationToken ct)
  {
    await HandleAsync(request, ct);
    return Unit.Value;
  }

  public async ValueTask HandleAsync(UserAuthenticated @event, CancellationToken ct = default)
  {
    await _controller.StartSession(ct);
  }

  public async ValueTask HandleAsync(UserLoggedOut @event, CancellationToken ct = default)
  {
    await _controller.StopSession(ct);
  }
}