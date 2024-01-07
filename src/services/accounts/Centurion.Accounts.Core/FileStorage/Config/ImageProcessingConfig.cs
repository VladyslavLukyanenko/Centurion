using Centurion.Accounts.Core.FileStorage.Image.ResizeStrategies;

namespace Centurion.Accounts.Core.FileStorage.Config;

public class ImageProcessingConfig
{
  public string TargetImageFormat { get; set; } = null!;
  public ImageSize Size { get; set; } = null!;
  public bool ResizeToFitExactSize { get; set; }
  public ImageResizeStrategy ResizeStrategy { get; set; }
}