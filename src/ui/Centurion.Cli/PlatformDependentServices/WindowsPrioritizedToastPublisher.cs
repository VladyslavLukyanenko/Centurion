using System.Diagnostics;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Services.ToastNotifications;

namespace Centurion.Cli.PlatformDependentServices;

public class WindowsPrioritizedToastPublisher : IPrioritizedToastPublisher
{
  private static readonly string NotifierExePath =
    Path.Combine(Environment.CurrentDirectory, "Centurion-Notifications.exe");
		
  public async ValueTask PublishAsync(ToastContent content, CancellationToken ct = default)
  {
    var process = Process.Start(new ProcessStartInfo(NotifierExePath)
    {
      Arguments = $"\"{content.Title}\" \"{content.Content}\""
    });

    if (process != null)
    {
      await process.WaitForExitAsync(ct);
    }
  }
}