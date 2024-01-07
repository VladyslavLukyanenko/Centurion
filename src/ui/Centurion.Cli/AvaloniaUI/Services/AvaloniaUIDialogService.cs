using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Centurion.Cli.Core.Services;

namespace Centurion.Cli.AvaloniaUI.Services;

public class AvaloniaUIDialogService : IDialogService
{
  public async ValueTask<string?> PickOpenFileAsync(string title, params string[] validExtensions)
  {
    if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime)
    {
      throw new InvalidOperationException("Unsupported windowing lifetime " + App.Current.GetType().Name);
    }

    var dialog = new OpenFileDialog
    {
      Title = title,
      Filters = new List<FileDialogFilter>
      {
        new()
        {
          Extensions = new List<string>(validExtensions.Select(FixLeadingDot)),
          Name = "Supported File Types"
        }
      }
    };

    var selectedFiles = await dialog.ShowAsync(lifetime.MainWindow);
    return selectedFiles?.FirstOrDefault();
  }


  public async ValueTask<string?> PickSaveFileAsync(string title, string ext, string? defaultFileName = null)
  {
    if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime)
    {
      throw new InvalidOperationException("Unsupported windowing lifetime " + App.Current.GetType().Name);
    }

    var dialog = new SaveFileDialog
    {
      Title = title,
      InitialFileName = defaultFileName,
      Filters = new List<FileDialogFilter>
      {
        new()
        {
          Extensions = new List<string>
          {
            FixLeadingDot(ext)
          },
          Name = "Supported File Types"
        }
      }
    };

    return await dialog.ShowAsync(lifetime.MainWindow);
  }

  public void ShowDirectory(string path)
  {
    if (!OperatingSystem.IsWindows())
    {
      throw new NotImplementedException("Not implemented yet");
    }

    Process.Start(new ProcessStartInfo
    {
      FileName = path,
      UseShellExecute = true,
      Verb = "open"
    });
  }

  private static string FixLeadingDot(string str) => str.StartsWith(".") ? str[1..] : str;
}