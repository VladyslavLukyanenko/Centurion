using Centurion.Contracts.Monitor.Integration;

namespace Centurion.Monitor.Domain;

public class WatcherStatusChangedEventArgs : AsyncEventArgs
{
  public WatcherStatusChangedEventArgs(MonitoringStatusChanged change, MonitorTarget target, CancellationToken ct) 
    : base(ct)
  {
    Change = change;
    Target = target;
  }

  public MonitoringStatusChanged Change { get; }
  public MonitorTarget Target { get; }
}