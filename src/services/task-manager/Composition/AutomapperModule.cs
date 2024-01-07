using AutoMapper;
using Centurion.TaskManager.Infrastructure;
using LightInject;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace Centurion.TaskManager.Composition;

public class AutomapperModule : ICompositionRoot
{
  public void Compose(IServiceRegistry serviceRegistry)
  {
    serviceRegistry.RegisterSingleton<Profile, GlobalMappingProfile>();
    serviceRegistry.RegisterScoped<IMapper, Mapper>();

    serviceRegistry.RegisterSingleton(factory =>
    {
      var mapperConfig = new MapperConfiguration(cfg =>
      {
        var profiles = factory.GetAllInstances<Profile>();
        foreach (var profile in profiles)
        {
          cfg.AddProfile(profile);
        }

        cfg.DisableConstructorMapping();
      });

      mapperConfig.AssertConfigurationIsValid();
      return (IConfigurationProvider) mapperConfig;
    });
  }
}