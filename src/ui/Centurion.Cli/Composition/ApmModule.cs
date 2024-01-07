using Autofac;
using Centurion.Cli.Core;
using Elastic.Apm;
using Elastic.Apm.Api;
using Elastic.Apm.Config;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

#nullable disable

namespace Centurion.Cli.Composition;

public class ApmModule : Module
{
  private readonly string _environment;

  public ApmModule(string environment)
  {
    _environment = environment;
  }

  protected override void Load(ContainerBuilder builder)
  {
    builder.Register(ctx =>
      {
        return ctx.Resolve(typeof(ILoggerFactory)) is ILoggerFactory loggerFactory
          ? (IApmLogger) ReflectionHelper.CreateInstanceOfInternalClass("Elastic.Apm.Extensions.Hosting",
            "Elastic.Apm.Extensions.Hosting", "NetCoreLogger", new object[] {loggerFactory})
          : (IApmLogger) ReflectionHelper.GetStaticInternalProperty("Elastic.Apm", "Elastic.Apm.Logging",
            "ConsoleLogger", "Instance");
      })
      .As<IApmLogger>()
      .SingleInstance();


    builder.Register(sp =>
      {
        IConfigurationRoot config = sp.Resolve<IConfigurationRoot>();
        return ReflectionHelper.CreateInstanceOfInternalClass("Elastic.Apm.Extensions.Hosting",
          "Elastic.Apm.Extensions.Hosting.Config",
          "MicrosoftExtensionsConfig", new object[]
          {
            config,
            sp.Resolve<IApmLogger>(),
            _environment
          }) as IConfigurationReader;
      })
      .As<IConfigurationReader>()
      .SingleInstance();


    builder.Register(sp =>
      {
        var components = new AgentComponents(sp.Resolve<IApmLogger>(), sp.Resolve<IConfigurationReader>());
        components.Service.Framework = new Framework
          {Name = ".NET Core", Version = Environment.Version.ToString(3)};
        components.Service.Language = new Language {Name = "C#"};
        return components;
      })
      .As<AgentComponents>()
      .SingleInstance();


    builder.Register(sp =>
      {
        var agentConfig = sp.Resolve<AgentComponents>();
        var subscribers = sp.Resolve<IEnumerable<IDiagnosticsSubscriber>>();
        var agent = (IApmAgent) ReflectionHelper.CreateInstanceOfInternalClass("Elastic.Apm", "Elastic.Apm",
          "ApmAgent", new object[] {agentConfig});
        agent.Subscribe(subscribers.ToArray());
        return agent;
      })
      .As<IApmAgent>()
      .SingleInstance();
    builder.Register(_ => new IDiagnosticsSubscriber[] {new HttpDiagnosticsSubscriber()});

    builder.Register(sp => sp.Resolve<IApmAgent>().Tracer)
      .As<ITracer>()
      .SingleInstance();
  }
}