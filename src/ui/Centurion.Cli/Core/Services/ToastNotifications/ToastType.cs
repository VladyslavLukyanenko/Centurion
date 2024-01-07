using Centurion.Cli.AvaloniaUI.Controls;

namespace Centurion.Cli.Core.Services.ToastNotifications;

public enum ToastType
{
  Information,
  Success,
  Warning,
  Error
}

public static class ToastTypeExtensions
{
  public static NotificationType ToNotificationType(this ToastType type) => type switch
  {
    ToastType.Information => NotificationType.Info,
    ToastType.Success => NotificationType.Success,
    ToastType.Warning => NotificationType.Warn,
    ToastType.Error => NotificationType.Error,
    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
  };
}