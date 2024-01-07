using Centurion.TaskManager.Application.Services;
using Centurion.TaskManager.Application.Services.Analytics;
using Centurion.TaskManager.Core.Events;
using Centurion.TaskManager.Core.Services;
using Centurion.TaskManager.Infrastructure.Services;
using Centurion.TaskManager.Web.Services;
using LightInject;
using Microsoft.AspNetCore.SignalR;

namespace Centurion.TaskManager.Composition;

public class ApplicationModule : ICompositionRoot
{
  public void Compose(IServiceRegistry serviceRegistry)
  {
    // core components
    serviceRegistry
      .RegisterScoped<IProductRepository, EfProductRepository>()
      // .RegisterScoped<ITaskRegistry, RedisBasedTaskRegistry>()
      .RegisterScoped<IEventRepository, EfEventRepository>()
      .RegisterScoped<ICheckoutTaskGroupRepository, EfCheckoutTaskGroupRepository>()
      .RegisterScoped<ICheckoutTaskRepository, EfCheckoutTaskRepository>();

    // app components
    serviceRegistry
      .RegisterScoped<ITaskManager, Application.Services.TaskManager>()
      .RegisterScoped<IProductProvider, EfProductProvider>()
      .RegisterScoped<IAnalyticsProvider, EfAnalyticsProvider>()
      .RegisterScoped<IPresetProvider, EfPresetProvider>()
      .RegisterScoped<ICheckoutTaskGroupProvider, EfCheckoutTaskGroupProvider>()
      .RegisterScoped<ICheckoutTaskProvider, EfCheckoutTaskProvider>();

    // web components
    serviceRegistry
      .RegisterScoped<IUserIdProvider, NameUserIdProvider>()
      .RegisterScoped<IUserInfoFactory, UserInfoFactory>()
      .RegisterSingleton<IBacklistedProductRepository, InMemoryBacklistedProductRepository>()
      .RegisterSingleton<ICheckoutClientFactory, CheckoutClientFactory>();
  }
}

public abstract class AbstractCloudModule<TCloudManager> : ICompositionRoot
  where TCloudManager : class, ICloudManager, ICloudConnectionPool, ICloudClient
{
  public void Compose(IServiceRegistry serviceRegistry)
  {
    serviceRegistry.RegisterSingleton<TCloudManager>()
      .RegisterSingleton<ICloudManager>(static _ => _.GetInstance<TCloudManager>())
      .RegisterSingleton<ICloudConnectionPool>(static _ => _.GetInstance<TCloudManager>())
      .RegisterSingleton<ICloudClient>(static _ => _.GetInstance<TCloudManager>());
  }
}

public class SingleNodeCloudModule : AbstractCloudModule<SingleNodeCloudManager>
{
}

public class DistributedCloudModule : AbstractCloudModule<DistributedCloudManager>
{
}