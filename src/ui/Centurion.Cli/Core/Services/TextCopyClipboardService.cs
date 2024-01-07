using TextCopy;

namespace Centurion.Cli.Core.Services;

public class TextCopyClipboardService : IClipboardService
{
  public async Task SetTextAsync(string text, CancellationToken ct = default)
  {
    await ClipboardService.SetTextAsync(text, ct);
  }
}