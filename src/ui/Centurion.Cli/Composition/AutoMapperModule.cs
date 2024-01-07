using Autofac;
using AutoMapper;
using AutoMapperProfile = AutoMapper.Profile;

namespace Centurion.Cli.Composition;

public class AutoMapperModule : Module
{
  protected override void Load(ContainerBuilder builder)
  {
    builder.RegisterType<Mapper>().As<IMapper>()
      .InstancePerLifetimeScope();

    builder.RegisterAssemblyTypes(GetType().Assembly)
      .Where(t => typeof(AutoMapperProfile).IsAssignableFrom(t))
      .As<AutoMapperProfile>()
      .SingleInstance();

    builder.Register<IConfigurationProvider>(ctx =>
      {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
          var profiles = ctx.Resolve<IEnumerable<AutoMapperProfile>>();
          foreach (var profile in profiles)
          {
            cfg.AddProfile(profile);
          }

          // cfg.DisableConstructorMapping();
        });

        mapperConfig.AssertConfigurationIsValid();
        return mapperConfig;
      })
      .SingleInstance();
  }
}