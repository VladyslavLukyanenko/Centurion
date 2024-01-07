using Centurion.Accounts.Core.FileStorage.FileSystem;
using Centurion.Accounts.Core.FileStorage.Image;
using Centurion.Accounts.Core.FileStorage.Image.ResizeStrategies;
using Centurion.Accounts.Infra.Services.FileSystem.Image.ResizeStrategies;
using SkiaSharp;

namespace Centurion.Accounts.Infra.Services.FileSystem.Image;

public class SkiaSharpImageOptimizationService
  : IImageOptimizationService
{
  private const SKEncodedImageFormat DefaultImageStoreFormat = SKEncodedImageFormat.Jpeg;

  private static readonly Dictionary<SKEncodedImageFormat, string> FormatToExtDict =
    new Dictionary<SKEncodedImageFormat, string>
    {
      {
        SKEncodedImageFormat.Png, ".png"
      },
      {
        SKEncodedImageFormat.Jpeg, ".jpg"
      }
    };

  private static readonly Dictionary<SKEncodedImageFormat, string> FormatToMimeDict =
    new Dictionary<SKEncodedImageFormat, string>
    {
      {
        SKEncodedImageFormat.Png, "image/png"
      },
      {
        SKEncodedImageFormat.Jpeg, "image/jpg"
      }
    };

  private static readonly Dictionary<string, SKEncodedImageFormat> ExtToFormatDict =
    new Dictionary<string, SKEncodedImageFormat>
    {
      {
        ".png", SKEncodedImageFormat.Png
      },
      {
        ".jpg", SKEncodedImageFormat.Jpeg
      },
      {
        ".jpeg", SKEncodedImageFormat.Jpeg
      }
    };

  private static readonly IDictionary<SKEncodedImageFormat, int> FormatToQualityDict =
    new Dictionary<SKEncodedImageFormat, int>
    {
      {
        SKEncodedImageFormat.Jpeg, 60
      },
      {
        SKEncodedImageFormat.Png, 100
      }
    };

  private readonly IImageResizeStrategyExecutorProvider _resizeStrategyExecutor;

  public SkiaSharpImageOptimizationService(IImageResizeStrategyExecutorProvider resizeStrategyExecutor)
  {
    _resizeStrategyExecutor = resizeStrategyExecutor;
  }

  public Task<IBinaryData> OptimizeAsync(OptimizationImageContext context, CancellationToken token = default)
  {
    using var skiaStream = new SKManagedStream(context.Image.OpenReadStream());
    using var codec = SKCodec.Create(skiaStream);
    using var bitmap = SKBitmap.Decode(codec);
    var resizedBitmap = ResizedImage(context, bitmap);


    var origin = codec.EncodedOrigin;

    switch (origin)
    {
      case SKEncodedOrigin.TopLeft:
        // this is default origin
        break;
      case SKEncodedOrigin.TopRight:
        resizedBitmap = FlipImage(resizedBitmap, false);
        break;
      case SKEncodedOrigin.BottomRight:
        resizedBitmap = Rotate(resizedBitmap, 180);
        break;
      case SKEncodedOrigin.BottomLeft:
        resizedBitmap = FlipImage(resizedBitmap, true);
        break;
      case SKEncodedOrigin.LeftTop:
        resizedBitmap = Rotate(resizedBitmap, -90);
        break;
      case SKEncodedOrigin.RightTop:
        resizedBitmap = Rotate(resizedBitmap, 90);
        break;
      case SKEncodedOrigin.RightBottom:
        resizedBitmap = Rotate(resizedBitmap, -180);
        break;
      case SKEncodedOrigin.LeftBottom:
        resizedBitmap = Rotate(resizedBitmap, 90);
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }


    var resizedImage = SKImage.FromBitmap(resizedBitmap);
    var format = GetImageFormat(context);
    var quality = GetTargetImageQuality(format);

    SKData data = resizedImage.Encode(format, quality);
    var fileName = GetFileNameWithExtension(context, format);

    IBinaryData result = new OptimizedImageData(fileName, FormatToMimeDict[format],
      new ImageSize(resizedBitmap.Width, resizedBitmap.Height), data.ToArray());

    return Task.FromResult(result);
  }

  // this magic taken from here: https://stackoverflow.com/questions/45077047/rotate-photo-with-skiasharp
  private static SKBitmap Rotate(SKBitmap bitmap, double angle)
  {
    var radians = Math.PI * angle / 180;
    var sine = (float) Math.Abs(Math.Sin(radians));
    var cosine = (float) Math.Abs(Math.Cos(radians));
    var originalWidth = bitmap.Width;
    var originalHeight = bitmap.Height;
    var rotatedWidth = (int) (cosine * originalWidth + sine * originalHeight);
    var rotatedHeight = (int) (cosine * originalHeight + sine * originalWidth);

    var rotatedBitmap = new SKBitmap(rotatedWidth, rotatedHeight);

    using (var surface = new SKCanvas(rotatedBitmap))
    {
      // ReSharper disable PossibleLossOfFraction
      surface.Translate(rotatedWidth / 2, rotatedHeight / 2);
      surface.RotateDegrees((float) angle);
      surface.Translate(-originalWidth / 2, -originalHeight / 2);
      // ReSharper restore PossibleLossOfFraction
      surface.DrawBitmap(bitmap, new SKPoint());
    }

    return rotatedBitmap;
  }

  private static SKBitmap FlipImage(SKBitmap source, bool horizontally)
  {
    var flippedBitmap = new SKBitmap(source.Width, source.Height);
    using (var surface = new SKCanvas(flippedBitmap))
    {
      if (horizontally)
      {
        surface.Scale(-1, 1);
        surface.Translate(source.Width, 0);
      }
      else
      {
        surface.Scale(1, -1);
        surface.Translate(0, source.Height);
      }

      surface.DrawBitmap(source, new SKPoint());
    }

    return flippedBitmap;
  }

  private SKBitmap ResizedImage(OptimizationImageContext context, SKBitmap bitmap)
  {
    if (context.ResizeEnabled)
    {
      var resizeExecutor = _resizeStrategyExecutor.Get(context.ResizeStrategy);
      bitmap = resizeExecutor?.Execute(context, bitmap) ?? throw new InvalidOperationException();
    }

    return bitmap;
  }

  private string GetFileNameWithExtension(OptimizationImageContext context, SKEncodedImageFormat format)
  {
    return context.Image.GetNameWithoutExtension() + FormatToExtDict[format];
  }

  private int GetTargetImageQuality(SKEncodedImageFormat format)
  {
    return FormatToQualityDict[format];
  }

  private SKEncodedImageFormat GetImageFormat(OptimizationImageContext context)
  {
    string ext = context.Image.GetExtension();
    if (!string.IsNullOrWhiteSpace(context.RestrictedOutputImageType))
    {
      ext = context.RestrictedOutputImageType;
    }

    if (!ExtToFormatDict.TryGetValue(ext.ToLowerInvariant(), out var format))
    {
      format = DefaultImageStoreFormat;
    }

    return format;
  }
}