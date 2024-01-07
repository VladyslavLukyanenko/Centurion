using System.Runtime.InteropServices;
using Autofac;
using Centurion.Cli.AvaloniaUI.Services;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Services.ToastNotifications;
using Centurion.Cli.PlatformDependentServices;
using IPrioritizedToastPublisher = Centurion.Cli.Core.Services.IPrioritizedToastPublisher;

namespace Centurion.Cli.Composition;

public class GuiServicesModule : Module
{
  protected override void Load(ContainerBuilder builder)
  {
    builder.RegisterAssemblyTypes(typeof(SecurityManager).Assembly)
      .InNotEmptyNamespaceOf<AvaloniaUIToastNotificationsManager>()
      .Where(_ => !_.IsAbstract && !_.IsNested)
      .AsImplementedInterfaces()
      .SingleInstance();

    Type publisherType;
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      publisherType = typeof(WindowsPrioritizedToastPublisher);
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
      publisherType = typeof(MacOSPrioritizedToastPublisher);
    }
    else
    {
      publisherType = typeof(NoopPrioritizedToastPublisher);
      //throw new NotSupportedException("Unsupported OS");
    }

    builder.RegisterType(publisherType)
      .As<IPrioritizedToastPublisher>()
      .SingleInstance();
  }

  private class NoopPrioritizedToastPublisher : IPrioritizedToastPublisher
  {
    public ValueTask PublishAsync(ToastContent content, CancellationToken ct = default)
    {
      return ValueTask.CompletedTask;
    }
  }
}