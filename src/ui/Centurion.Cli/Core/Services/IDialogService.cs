namespace Centurion.Cli.Core.Services;

public interface IDialogService
{
  ValueTask<string?> PickOpenFileAsync(string title, params string[] validExtensions);
  ValueTask<string?> PickSaveFileAsync(string title, string ext, string? defaultFileName = null);
  void ShowDirectory(string path);
}