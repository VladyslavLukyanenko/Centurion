using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.ViewModels;

namespace Centurion.Cli.AvaloniaUI.Services;

public class AvaloniaUIBasedSmsConfirmationManager : ISmsConfirmationManager
{
  public async ValueTask<string?> Prompt(string taskDisplayId, string phoneNumber)
  {
    if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime)
    {
      throw new InvalidOperationException(
        $"Application lifetime '{Application.Current.ApplicationLifetime.GetType().Name}' not supported");
    }

    var vm = new SmsConfirmationViewModel(taskDisplayId, phoneNumber);
    var tcs = new TaskCompletionSource<string?>();
    await Dispatcher.UIThread.InvokeAsync(() =>
    {
      var view = (Window)vm.LocateGuiView();
      var mainWindow = lifetime.MainWindow;
      view.Show(mainWindow);

      view.Closed += ViewOnClosed;
      mainWindow.Closed += MainWindowOnClosed;

      void ViewOnClosed(object? sender, EventArgs e)
      {
        view.Closed -= ViewOnClosed;
        tcs.TrySetResult(vm.Code);
        mainWindow.Closed -= MainWindowOnClosed;
      }

      void MainWindowOnClosed(object? sender, EventArgs e)
      {
        tcs.TrySetCanceled();
        view.Close();
      }
    });


    return await tcs.Task;
  }
}