using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace Centurion.Cli.AvaloniaUI;

internal static class ApplicationExtensions
{
  public static void Shutdown(this Application app)
  {
    if (app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime cl)
    {
      cl.Shutdown();
      LifetimeUtil.GracefullyTerminateProcesses().ConfigureAwait(false).GetAwaiter().GetResult();
    }
    
    Environment.Exit(0);
  }
}