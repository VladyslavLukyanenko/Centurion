using Centurion.Accounts.Core.FileStorage.Image;
using Centurion.Accounts.Core.FileStorage.Image.ResizeStrategies;
using SkiaSharp;

namespace Centurion.Accounts.Infra.Services.FileSystem.Image.ResizeStrategies;

public class ImageResizeStrategyCropExecutor : IImageResizeStrategyExecutor
{
  public ImageResizeStrategy Strategy => ImageResizeStrategy.Crop;

  public SKBitmap Execute(OptimizationImageContext context, SKBitmap convertedBitmap)
  {
    if (!context.ResizeEnabled)
    {
      throw new InvalidOperationException("Resize disabled");
    }

    ImageSize size = context.MaxSize;
    var height = size.Height;
    var width = size.Width;
    var scaleRequired = context.ResizeToFitExactSize
                        && (convertedBitmap.Height != size.Height || convertedBitmap.Width != size.Width)
                        || convertedBitmap.Height > height
                        || convertedBitmap.Width > width;
    if (!scaleRequired)
    {
      return convertedBitmap;
    }

    var sourceBitmap = ScaleImage(width, height, convertedBitmap);
    var isNotRectangle = sourceBitmap.Height != sourceBitmap.Width;

    return isNotRectangle
      ? ExtractImageSubset(sourceBitmap, width, height)
      : sourceBitmap;
  }

  private static SKBitmap ExtractImageSubset(SKBitmap sourceBitmap, int targetWidth, int targetHeight)
  {
    SKRectI subsetSize;
    if (targetWidth < sourceBitmap.Width)
    {
      var widthReminder = sourceBitmap.Width - targetWidth;
      var leftShift = widthReminder / 2;

      subsetSize = SKRectI.Create(leftShift, 0, targetWidth, targetHeight);
    }
    else
    {
      var heightReminder = sourceBitmap.Height - targetHeight;
      var topShift = heightReminder / 2;

      subsetSize = SKRectI.Create(0, topShift, sourceBitmap.Width, targetHeight);
    }

    var dst = new SKBitmap();
    if (!sourceBitmap.ExtractSubset(dst, subsetSize))
    {
      throw new InvalidOperationException();
    }

    return dst;
  }

  private static SKBitmap ScaleImage(int width, int height, SKBitmap sourceBitmap)
  {
    var widthFactor = width / (double) sourceBitmap.Width;
    height = (int) Math.Round(sourceBitmap.Height * widthFactor);
    // if (sourceBitmap.Width > sourceBitmap.Height)
    // {
    //   var heightFactor = height / (double) sourceBitmap.Height;
    //   width = (int) Math.Round(sourceBitmap.Width * heightFactor);
    // }
    // else
    // {
    //   var widthFactor = width / (double) sourceBitmap.Width;
    //   height = (int) Math.Round(sourceBitmap.Height * widthFactor);
    // }

    var scaleInfo = new SKImageInfo(width, height);
    return sourceBitmap.Resize(scaleInfo, SKFilterQuality.High);
  }
}