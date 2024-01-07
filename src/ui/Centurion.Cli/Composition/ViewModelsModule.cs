using Autofac;
using System.Reflection;
using Centurion.Cli.Core.Composition;
using Centurion.Cli.Core.ViewModels;
using ReactiveUI;

namespace Centurion.Cli.Composition;

public class ViewModelsModule : Autofac.Module
{
  protected override void Load(ContainerBuilder builder)
  {
    var ns = typeof(ViewModelBase).Namespace;
    var viewModelTypes = typeof(ViewModelBase).Assembly
      .ExportedTypes
      .Where(_ => !_.IsAbstract && !_.IsNested)
      .Where(t => !string.IsNullOrEmpty(ns) && t.IsInNamespace(ns))
      .Where(t => t.IsAssignableTo<ViewModelBase>());

    foreach (var vmType in viewModelTypes)
    {
      var attr = vmType.GetCustomAttribute<TransientViewModelAttribute>();
      var registerBuilder = builder.RegisterType(vmType)
        .AsSelf()
        .AsImplementedInterfaces();

      if (attr is not null)
      {
        registerBuilder.InstancePerDependency();
      }
      else
      {
        registerBuilder.SingleInstance();
      }
    }

    builder.RegisterType<RoutingState>()
      .As<RoutingState>()
      .SingleInstance();

    builder.Register(ctx => ctx.Resolve<MainViewModel>())
      .As<IScreen>()
      .SingleInstance();
  }
}