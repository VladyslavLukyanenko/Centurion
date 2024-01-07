using Centurion.Contracts;
using Centurion.Monitor.Domain.Services;
using LightInject;

namespace Centurion.Monitor.Infra;

public class LightInjectBasedStoreMonitorFactory : IStoreMonitorFactory
{
  private readonly IServiceFactory _container;

  public LightInjectBasedStoreMonitorFactory(IServiceFactory container)
  {
    _container = container;
  }

  public IStoreMonitor CreateFor(Module module) => _container.GetInstance<IStoreMonitor>(module.ToString());
}