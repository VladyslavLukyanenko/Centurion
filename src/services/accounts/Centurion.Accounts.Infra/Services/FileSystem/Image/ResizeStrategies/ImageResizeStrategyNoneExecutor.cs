using Centurion.Accounts.Core.FileStorage.Image;
using Centurion.Accounts.Core.FileStorage.Image.ResizeStrategies;
using SkiaSharp;

namespace Centurion.Accounts.Infra.Services.FileSystem.Image.ResizeStrategies;

public class ImageResizeStrategyNoneExecutor
  : IImageResizeStrategyExecutor
{
  public ImageResizeStrategy Strategy => ImageResizeStrategy.None;

  public SKBitmap Execute(OptimizationImageContext context, SKBitmap convertedBitmap)
  {
    return convertedBitmap;
  }
}