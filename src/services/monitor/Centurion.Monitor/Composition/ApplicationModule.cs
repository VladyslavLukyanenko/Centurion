using System.Reflection;
using System.Text.Json;
using Centurion.Monitor.App;
using Centurion.Monitor.Domain.Antibot;
using Centurion.Monitor.Domain.Services;
using Centurion.Monitor.Infra;
using Elastic.Apm;
using LightInject;

namespace Centurion.Monitor.Composition;

public class ApplicationModule : ICompositionRoot
{
  public void Compose(IServiceRegistry serviceRegistry)
  {
    serviceRegistry
      .RegisterScoped<IWatcherFactory, DiBasedWatcherFactory>()
      .RegisterScoped<IStoreMonitorFactory, LightInjectBasedStoreMonitorFactory>()
      .RegisterScoped<IAntibotProtectionSolverProvider, DiBasedAntibotProtectionSolverProvider>()
      .RegisterSingleton(_ => new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      })
      .RegisterTransient<IWatcher, GenericWatcher>();

    serviceRegistry.RegisterTransient(_ => Agent.Tracer);
      
    var monitorTypes = typeof(MonitorAttribute).Assembly
      .DefinedTypes
      .Where(type => type.IsAssignableTo(typeof(IStoreMonitor)))
      .Where(t => !t.IsAbstract)
      .ToArray();

    foreach (var type in monitorTypes)
    {
      var monitorAttr = type.GetCustomAttribute<MonitorAttribute>();
      if (monitorAttr == null)
      {
        throw new InvalidOperationException(
          $"Missing required attribute {nameof(MonitorAttribute)} for {type.Name}");
      }

      serviceRegistry.RegisterTransient(typeof(IStoreMonitor), type, monitorAttr.MonitorSlug);
    }
  }
}