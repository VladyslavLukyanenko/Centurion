using Microsoft.Extensions.DependencyInjection;

namespace Centurion.Monitor.Domain.Services;

public class DiBasedWatcherFactory : IWatcherFactory
{
  private readonly IServiceProvider _serviceProvider;

  public DiBasedWatcherFactory(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
  }

  public IWatcher Create(MonitorTarget target) => _serviceProvider.GetRequiredService<IWatcher>();
}