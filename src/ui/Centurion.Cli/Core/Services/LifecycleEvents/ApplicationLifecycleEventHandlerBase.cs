using Centurion.Cli.Core.Domain.LifecycleEvents;
using MediatR;

namespace Centurion.Cli.Core.Services.LifecycleEvents;

public abstract class ApplicationLifecycleEventHandlerBase<T> : IApplicationLifecycleEventHandler<T>
  where T : IApplicationLifecycleEvent
{
  public abstract ValueTask HandleAsync(T @event, CancellationToken ct = default);

  async Task<Unit> IRequestHandler<T, Unit>.Handle(T request, CancellationToken cancellationToken)
  {
    await HandleAsync(request, cancellationToken);
    return Unit.Value;
  }
}