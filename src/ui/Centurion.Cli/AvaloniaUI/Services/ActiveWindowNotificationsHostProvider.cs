using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;

namespace Centurion.Cli.AvaloniaUI.Services;

public class ActiveWindowNotificationsHostProvider : INotificationsHostProvider
{
  private static readonly string HostName = "NotificationsHost";

  public StackPanel GetHost()
  {
    var mainWindow = ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
    return FindHost(mainWindow) ?? AppendHostAndGet(mainWindow);
  }

  private static StackPanel? FindHost(Window host)
  {
    if (host.Content is IControl c)
    {
      return c.FindControl<StackPanel>(HostName);
    }
    
    return null;
  }

  private StackPanel AppendHostAndGet(Window host)
  {
    if (host.Content is not IControl c)
    {
      throw new InvalidOperationException(
        $"Main window's content is not control so we can't append it to our grid. Please add a StackPanel with name {HostName} to root container to host notifications there.");
    }

    var panel = new StackPanel
    {
      VerticalAlignment = VerticalAlignment.Bottom,
      HorizontalAlignment = HorizontalAlignment.Right,
      Margin = new Thickness(10),
      Spacing = 10,
      Name = HostName,
      ZIndex = int.MaxValue
    };

    Grid? rootGrid = host.Content as Grid;
    if (rootGrid is null)
    {
      rootGrid = new Grid();
      host.Content = rootGrid;
      rootGrid.Children.Add(c);
    }

    rootGrid.Children.Add(panel);

    return panel;
  }
}