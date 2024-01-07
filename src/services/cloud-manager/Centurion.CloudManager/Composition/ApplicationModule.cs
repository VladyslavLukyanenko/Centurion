using Centurion.CloudManager.App.Services;
using Centurion.CloudManager.Domain;
using Centurion.CloudManager.Domain.Services;
using Centurion.CloudManager.Infra.Repositories;
using Centurion.CloudManager.Infra.Services;
using Centurion.CloudManager.Infra.Services.Aws;
using Centurion.CloudManager.Web.Services;
using LightInject;

namespace Centurion.CloudManager.Composition;

public class ApplicationModule : ICompositionRoot
{
  public void Compose(IServiceRegistry serviceRegistry)
  {
    // core components
    serviceRegistry.RegisterScoped<IImageInfoRepository, EfImageInfoRepository>()
      .RegisterScoped<INodeSnapshotRepository, EfNodeSnapshotRepository>()
      .RegisterScoped<IImagesManager, ImagesManager>()
      .RegisterScoped<IInfrastructureClient, AwsInfrastructureClient>()
      // .AddScoped<IInfrastructureClient, GoogleComputeEngineInfrastructureClient>()
      .RegisterScoped<IDockerManager, DockerManager>()
      .RegisterScoped<IDockerAuthProvider, LoginPwdDockerAuthProvider>()
      .RegisterScoped<IImagesRuntimeInfoService, ImagesRuntimeInfoService>()
      .RegisterScoped<INodeLifetimeManager, NodeLifetimeManager>()
      .RegisterScoped<ICloudManager, AwsCloudManager>();

    // app components
    serviceRegistry
      .RegisterSingleton<IDockerClientsProvider, DockerClientsProvider>()
      .RegisterSingleton<IComponentsStateRegistry, InMemoryComponentsStateRegistry>()
      .RegisterSingleton<IExecutionScheduler, TokenBucketExecutionScheduler>()

      .RegisterScoped<IScopedBackgroundService, NodesOrchestratorWorker>();
  }
}