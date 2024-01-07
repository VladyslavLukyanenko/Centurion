using Autofac;
using Centurion.Accounts.App.Identity.Services;
using Centurion.Accounts.Authorization;
using Centurion.Accounts.Core.Identity;

namespace Centurion.Accounts.Foundation.Composition;

public class AppModule : Module
{
  protected override void Load(ContainerBuilder builder)
  {
    builder.RegisterAssemblyTypes(typeof(User).Assembly)
      .Where(_ => _.Namespace?.Contains(".Services") ?? false)
      .AsImplementedInterfaces()
      .InstancePerLifetimeScope();

    builder.RegisterAssemblyTypes(typeof(AspNetIdentityUserManager).Assembly)
      .AsImplementedInterfaces()
      .InstancePerLifetimeScope();

    builder.RegisterAssemblyTypes(GetType().Assembly)
      .Except<MvcPermissionsRegistry>()
      .AsImplementedInterfaces()
      .InstancePerLifetimeScope();

    builder.RegisterType<MvcPermissionsRegistry>()
      .AsImplementedInterfaces()
      .SingleInstance();

    //
    // builder.RegisterAssemblyTypes(typeof(UserData).Assembly)
    //   .InNamespaceOf<AspNetIdentityUserManager>()
    //   .AsImplementedInterfaces()
    //   .InstancePerLifetimeScope();
    //
    // builder.RegisterAssemblyTypes(typeof(UserService).Assembly)
    //   .InNamespaceOf<UserService>()
    //   .AsImplementedInterfaces()
    //   .InstancePerLifetimeScope();
  }
}