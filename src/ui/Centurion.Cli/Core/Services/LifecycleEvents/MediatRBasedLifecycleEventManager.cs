using Centurion.Cli.Core.Domain.LifecycleEvents;
using MediatR;

namespace Centurion.Cli.Core.Services.LifecycleEvents;

public class MediatRBasedLifecycleEventManager : ILifecycleEventManager
{
  private readonly IMediator _mediator;

  public MediatRBasedLifecycleEventManager(IMediator mediator)
  {
    _mediator = mediator;
  }

  public async ValueTask DispatchAsync<T>(T @event, CancellationToken ct = default)
    where T : IApplicationLifecycleEvent
  {
    await _mediator.Send(@event, ct);
  }
}