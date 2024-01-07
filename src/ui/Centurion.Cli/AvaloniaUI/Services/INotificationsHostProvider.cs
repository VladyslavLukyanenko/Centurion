using Avalonia.Controls;

namespace Centurion.Cli.AvaloniaUI.Services;

public interface INotificationsHostProvider
{
  StackPanel GetHost();
}