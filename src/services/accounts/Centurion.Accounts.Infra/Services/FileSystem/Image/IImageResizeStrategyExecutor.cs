using Centurion.Accounts.Core.FileStorage.Image;
using Centurion.Accounts.Core.FileStorage.Image.ResizeStrategies;
using SkiaSharp;

namespace Centurion.Accounts.Infra.Services.FileSystem.Image;

public interface IImageResizeStrategyExecutor
{
  ImageResizeStrategy Strategy { get; }
  SKBitmap Execute(OptimizationImageContext context, SKBitmap convertedBitmap);
}