using Centurion.Contracts.Monitor.Integration;

namespace Centurion.Monitor.Domain.Services;

public interface IStoreMonitor : IAsyncDisposable
{
  bool IsInitialized { get; }
  IAsyncEnumerable<MonitoringStatusChanged> Initialize(MonitorTarget target, CancellationToken ct);
  IAsyncEnumerable<MonitoringStatusChanged> Monitor(MonitorTarget target, CancellationToken ct);
}