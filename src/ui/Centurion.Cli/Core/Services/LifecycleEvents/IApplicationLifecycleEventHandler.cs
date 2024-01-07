using Centurion.Cli.Core.Domain.LifecycleEvents;
using MediatR;

namespace Centurion.Cli.Core.Services.LifecycleEvents;

public interface IApplicationLifecycleEventHandler<in T> : IRequestHandler<T>
  where T : IApplicationLifecycleEvent
{
  ValueTask HandleAsync(T @event, CancellationToken ct = default);
}