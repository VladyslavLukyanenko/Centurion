using Autofac;
using Centurion.Cli.Core.Validators;

namespace Centurion.Cli.Composition;

public class FluentValidatorsModule : Module
{
  protected override void Load(ContainerBuilder builder)
  {
    builder.RegisterAssemblyTypes(typeof(ProfileValidator).Assembly)
      .InNotEmptyNamespaceOf<ProfileValidator>()
      .Where(_ => !_.IsAbstract && !_.IsNested)
      .AsSelf()
      .SingleInstance();
  }
}