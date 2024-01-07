using Centurion.Cli.Core.Services.ToastNotifications;

namespace Centurion.Cli.Core.Services;

public interface IPrioritizedToastPublisher
{
  ValueTask PublishAsync(ToastContent content, CancellationToken ct = default);
}