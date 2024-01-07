using Centurion.Contracts;

namespace Centurion.Monitor.Domain.Services;

public interface IStoreMonitorFactory
{
  IStoreMonitor CreateFor(Module module);
}