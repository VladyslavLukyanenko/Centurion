using Centurion.Cli.Core.Domain.LifecycleEvents;

namespace Centurion.Cli.Core.Services.LifecycleEvents;

public interface ILifecycleEventManager
{
  ValueTask DispatchAsync<T>(T @event, CancellationToken ct = default) where T : IApplicationLifecycleEvent;
}