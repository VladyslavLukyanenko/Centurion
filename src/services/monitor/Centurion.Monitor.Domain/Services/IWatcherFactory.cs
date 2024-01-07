namespace Centurion.Monitor.Domain.Services;

public interface IWatcherFactory
{
  IWatcher Create(MonitorTarget target);
}