using Centurion.Cli.Core.Domain;
using MediatR;

namespace Centurion.Cli.Core.Services.NotificationHandlers;

public abstract class ApplicationNotificationHandlerBase<T>
  : IApplicationNotificationHandler<T>
  where T : IApplicationNotification
{
  public abstract ValueTask HandleAsync(T @event, CancellationToken ct);

  async Task INotificationHandler<T>.Handle(T request, CancellationToken cancellationToken)
  {
    await HandleAsync(request, cancellationToken);
  }
}