using System.Text.RegularExpressions;

#pragma warning disable 8618

namespace Centurion.Accounts.Core.FileStorage.Config;

public class FileUploadsConfig
{
  private Regex? _imageMimeTypeCheckRegex;

  public string SupportedFileTypes { get; set; }
  public string StoreName { get; set; }
  public bool GenerateRandomFileName { get; set; } = true;
  public bool AppendHashToFileName { get; set; } = false;
  public ImageProcessingConfig? ImageProcessing { get; set; }

  public bool IsFileTypeSupported(string mimeType)
  {
    if (_imageMimeTypeCheckRegex == null)
    {
      _imageMimeTypeCheckRegex =
        new Regex(SupportedFileTypes, RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    return _imageMimeTypeCheckRegex.IsMatch(mimeType);
  }
}