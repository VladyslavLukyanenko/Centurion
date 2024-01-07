using Autofac;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Services.Harvesters;

namespace Centurion.Cli.Composition;

public class CoreServicesModule : Module
{
  protected override void Load(ContainerBuilder builder)
  {
    builder.RegisterAssemblyTypes(typeof(IAppStateHolder).Assembly)
      .InNotEmptyNamespaceOf<IAppStateHolder>()
      .Except<PuppeteerBasedHarvester>()
      .Except<PuppeteerBased3DS2Solver>()
      .Except<PuppeteerHandle>()
      .Where(_ => !_.IsAbstract && !_.IsNested)
      .AsImplementedInterfaces()
      .SingleInstance();

    builder.RegisterType<PuppeteerBasedHarvester>()
      .As<IHarvester>()
      .InstancePerDependency();

    builder.RegisterType<PuppeteerBased3DS2Solver>()
      .As<I3DS2Solver>()
      .InstancePerDependency();

    builder.RegisterType<PuppeteerHandle>()
      .As<IPuppeteerHandle>()
      .InstancePerDependency();
  }
}