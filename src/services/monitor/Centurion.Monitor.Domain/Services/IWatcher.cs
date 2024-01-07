using Centurion.Contracts.Monitor.Integration;

namespace Centurion.Monitor.Domain.Services;

public interface IWatcher : IAsyncDisposable
{
  MonitorTarget? Target { get; }
  ValueTask SpawnAsync(MonitorTarget target, CancellationToken ct);

  IAsyncEnumerable<MonitoringStatusChanged> ExecuteMonitoringIteration(MonitorTarget target,
    CancellationToken ct = default);

  event AsyncEventHandler<WatcherStatusChangedEventArgs> StatusChanged;
}