using Autofac;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.StaticFiles;
using Centurion.Accounts.Infra;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace Centurion.Accounts.Foundation.Composition;

public class InfraModule : Module
{
  protected override void Load(ContainerBuilder builder)
  {
    builder.RegisterType<FileExtensionContentTypeProvider>().As<IContentTypeProvider>().InstancePerLifetimeScope();
    builder.RegisterAssemblyTypes(typeof(IDataSeeder).Assembly)
      .AsImplementedInterfaces()
      .InstancePerLifetimeScope();

    // registers bus consumers 
    builder.RegisterAssemblyTypes(typeof(AccountsDbContext).Assembly)
      .AsClosedTypesOf(typeof(IConsumer<>));

    builder.RegisterType<Mapper>().As<IMapper>().InstancePerLifetimeScope();
    builder.RegisterAssemblyTypes(typeof(EntitiesMappingProfile).Assembly)
      .Where(t => typeof(Profile).IsAssignableFrom(t))
      .As<Profile>()
      .SingleInstance();

    builder.Register<IConfigurationProvider>(ctx =>
      {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
          var profiles = ctx.Resolve<IEnumerable<Profile>>();
          foreach (var profile in profiles)
          {
            cfg.AddProfile(profile);
          }

          cfg.DisableConstructorMapping();
        });

        mapperConfig.AssertConfigurationIsValid();
        return mapperConfig;
      })
      .SingleInstance();
  }
}