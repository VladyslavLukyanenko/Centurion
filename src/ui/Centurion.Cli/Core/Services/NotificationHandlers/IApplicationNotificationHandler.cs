using Centurion.Cli.Core.Domain;
using MediatR;

namespace Centurion.Cli.Core.Services.NotificationHandlers;

public interface IApplicationNotificationHandler<in T> : INotificationHandler<T>
  where T: IApplicationNotification
{
  ValueTask HandleAsync(T eventMessage, CancellationToken ct = default);
}