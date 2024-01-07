using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Centurion.Cli.AvaloniaUI.Views;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.ViewModels;
using Splat;

namespace Centurion.Cli.AvaloniaUI.Services;

public class AvaloniaUIGlobalRouter : IGlobalRouter
{
  private readonly IReadonlyDependencyResolver _resolver;

  public AvaloniaUIGlobalRouter(IReadonlyDependencyResolver resolver)
  {
    _resolver = resolver;
  }

  public async ValueTask ShowTransitionView(bool authenticated)
  {
    if (Application.Current is null)
    {
      return;
    }

    var lifetime = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
    await Dispatcher.UIThread.InvokeAsync(() =>
    {
      var wnd = new SplashView(authenticated);
      DisplayWindow(lifetime, wnd);
    });
  }

  public async ValueTask ShowLoginView() => await Display<LoginViewModel>();
  public async ValueTask ShowMainView() => await Display<MainViewModel>();

  private async ValueTask Display<T>() where T : ViewModelBase
  {
    if (Application.Current is null)
    {
      return;
    }

    var loginVm = _resolver.GetService<T>()!;
    var lifetime = (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
    await Dispatcher.UIThread.InvokeAsync(() =>
    {
      var window = (Window)loginVm.LocateGuiView();
      DisplayWindow(lifetime, window);
    });
  }

  private static void DisplayWindow(IClassicDesktopStyleApplicationLifetime lifetime, Window wnd)
  {
    if (lifetime.MainWindow is not null && lifetime.MainWindow.GetType() == wnd.GetType())
    {
      return;
    }

    var oldWnd = lifetime.MainWindow;
    lifetime.MainWindow = wnd;
    wnd.Show();
    oldWnd?.Close();
  }
}