using Centurion.Accounts.Core.FileStorage.Image;
using Centurion.Accounts.Core.FileStorage.Image.ResizeStrategies;
using SkiaSharp;

namespace Centurion.Accounts.Infra.Services.FileSystem.Image.ResizeStrategies;

public abstract class ImageResizeStrategyByWidthHeightExecutor
  : IImageResizeStrategyExecutor
{
  protected abstract ImageResizeBy ResizeBy { get; }
  public abstract ImageResizeStrategy Strategy { get; }

  public SKBitmap Execute(OptimizationImageContext context, SKBitmap convertedBitmap)
  {
    if (!context.ResizeEnabled)
    {
      throw new InvalidOperationException("Resize disabled");
    }

    ImageSize size = context.MaxSize;

    if (!ResizeRequired(convertedBitmap, size, context))
    {
      return convertedBitmap;
    }

    var changedSize = ImageSizeUtil.Resize(new ImageSize(convertedBitmap.Width, convertedBitmap.Height),
      GetDestinationValue(size), ResizeBy);

    var changedImageInfo = new SKImageInfo(changedSize.Width, changedSize.Height, convertedBitmap.ColorType,
      convertedBitmap.AlphaType, convertedBitmap.ColorSpace);

    return convertedBitmap.Resize(changedImageInfo, SKFilterQuality.High);
  }

  protected abstract int GetDestinationValue(ImageSize size);

  protected abstract bool ResizeRequired(SKBitmap convertedBitmap, ImageSize size, OptimizationImageContext context);
}