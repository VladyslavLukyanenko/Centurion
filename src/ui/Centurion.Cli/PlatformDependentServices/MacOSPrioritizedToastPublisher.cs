using Centurion.Cli.Core;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Services.ToastNotifications;

namespace Centurion.Cli.PlatformDependentServices;

public class MacOSPrioritizedToastPublisher : IPrioritizedToastPublisher
{
  public ValueTask PublishAsync(ToastContent content, CancellationToken ct = default)
  {
    PlatformInteropUtils.Bash(
      $"osascript -e 'display notification \"{content.Content}\" with title \"{content.Title}\"'");

    return default;
  }
}