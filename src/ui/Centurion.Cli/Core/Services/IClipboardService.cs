namespace Centurion.Cli.Core.Services;

public interface IClipboardService
{
  Task SetTextAsync(string text, CancellationToken ct = default);
}