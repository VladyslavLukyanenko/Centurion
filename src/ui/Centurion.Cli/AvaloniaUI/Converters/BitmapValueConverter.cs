using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Skia;
using Avalonia.Visuals.Media.Imaging;
using SkiaSharp;

namespace Centurion.Cli.AvaloniaUI.Converters;

public class BitmapValueConverter : IValueConverter
{
  private const int MaxWidth = 150;
  private static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name!;
  public static readonly IValueConverter Instance = new BitmapValueConverter();
  private static readonly HttpClient HttpClient = new();
  private static readonly ConcurrentDictionary<Uri, Lazy<Task<IBitmap>>> PicturesCache = new();

  public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value is string path && targetType == typeof(IBitmap))
    {
      if (!path.StartsWith("http") && !path.StartsWith("avares"))
      {
        path = $"avares://{AssemblyName}{path}";
      }

      var uri = new Uri(path, UriKind.RelativeOrAbsolute);

      return Task.Run(() => LoadPicture(uri)).GetAwaiter().GetResult();
    }

    return null;
  }

  private async Task<IBitmap?> LoadPicture(Uri uri)
  {
    return await PicturesCache.GetOrAdd(uri, picUri => new Lazy<Task<IBitmap>>(async () => uri.Scheme switch
    {
      "file" => new Bitmap(picUri.OriginalString),
      "http" => await DownloadPicture(picUri),
      "https" => await DownloadPicture(picUri),
      _ => ReadFromAssets(picUri)
    })).Value;
  }

  private static IBitmap ReadFromAssets(Uri uri)
  {
    var assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!;
    return new Bitmap(assets.Open(uri));
  }

  private static async Task<IBitmap> DownloadPicture(Uri uri)
  {
    var r = await HttpClient.GetAsync(uri);
    var pictureStream = await r.Content.ReadAsStreamAsync();

    SKCodec codec = SKCodec.Create(pictureStream);
    SKImageInfo info = codec.Info;
    if (info.Width < MaxWidth)
    {
      pictureStream.Position = 0;
      return new Bitmap(pictureStream);
    }

    var scale = MaxWidth / (double)info.Width;
    var destinationSize = new PixelSize((int)(info.Width * scale), (int)(info.Height * scale));
    SKSizeI supportedScale = codec.GetScaledDimensions((float)destinationSize.Width / info.Width);

    SKImageInfo nearest = new SKImageInfo(supportedScale.Width, supportedScale.Height);
    SKBitmap bmp = SKBitmap.Decode(codec, nearest);

    SKImageInfo desired = new SKImageInfo(destinationSize.Width, destinationSize.Height, SKColorType.Bgra8888);
    bmp = bmp.Resize(desired, BitmapInterpolationMode.HighQuality.ToSKFilterQuality());

    SKImage image = SKImage.FromBitmap(bmp);

    var picture = new Bitmap(image.Encode().AsStream());
    return picture;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotSupportedException();
  }
}