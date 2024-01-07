using Autofac;
using MediatR;
using MediatR.Pipeline;

namespace Centurion.Cli.Composition;

public class MediatRModule : Module
{
  protected override void Load(ContainerBuilder builder)
  {
    builder
      .RegisterType<Mediator>()
      .As<IMediator>()
      .InstancePerLifetimeScope();

    // request & notification handlers
    builder.Register<ServiceFactory>(context =>
    {
      var c = context.Resolve<IComponentContext>();
      return t => c.Resolve(t);
    });


    var mediatrOpenTypes = new[]
    {
      typeof(IRequestHandler<,>),
      typeof(IRequestExceptionHandler<,,>),
      typeof(IRequestExceptionAction<,>),
      typeof(INotificationHandler<>),
    };

    foreach (var mediatrOpenType in mediatrOpenTypes)
    {
      builder
        .RegisterAssemblyTypes(GetType().Assembly)
        .AsClosedTypesOf(mediatrOpenType)
        // when having a single class implementing several handler types
        // this call will cause a handler to be called twice
        // in general you should try to avoid having a class implementing for instance `IRequestHandler<,>` and `INotificationHandler<>`
        // the other option would be to remove this call
        // see also https://github.com/jbogard/MediatR/issues/462
        .AsImplementedInterfaces();
    }

    // It appears Autofac returns the last registered types first
    builder.RegisterGeneric(typeof(RequestPostProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
    builder.RegisterGeneric(typeof(RequestPreProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
    builder.RegisterGeneric(typeof(RequestExceptionActionProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
    builder.RegisterGeneric(typeof(RequestExceptionProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
  }
}