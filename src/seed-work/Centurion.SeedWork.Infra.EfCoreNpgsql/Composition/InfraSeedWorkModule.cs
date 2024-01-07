using Centurion.SeedWork.Infra.EfCoreNpgsql.Services;
using Centurion.SeedWork.Primitives;
using LightInject;
using MediatR;
using NodaTime;

namespace Centurion.SeedWork.Infra.EfCoreNpgsql.Composition;

public class InfraSeedWorkModule : ICompositionRoot
{
  public void Compose(IServiceRegistry serviceRegistry)
  {
    // core components
    serviceRegistry.RegisterScoped<IUnitOfWork, DbContextBasedUnitOfWork>();

    // infra components
    serviceRegistry
      .RegisterScoped<IEfAuditableEntitiesUpdater, EfAuditableEntitiesUpdater<string>>()
      .RegisterSingleton<IClock>(_ => SystemClock.Instance)
      .RegisterScoped<IUnitOfWork, DbContextBasedUnitOfWork>();

    serviceRegistry.Register<IMediator, Mediator>();
    serviceRegistry.RegisterAssembly(GetType().Assembly, (serviceType, _) =>
    {
      return IsHandler(serviceType);

      static bool IsHandler(Type? type)
      {
        if (type is null)
        {
          return false;
        }

        return type is { IsConstructedGenericType: true } && (
          type.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
          type.GetGenericTypeDefinition() == typeof(INotificationHandler<>)
          // ReSharper disable once ConstantConditionalAccessQualifier
        ) || IsHandler(type?.BaseType);
      }
    });

    serviceRegistry.Register<ServiceFactory>(fac => fac.GetInstance);
  }
}