using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Skia;
using Avalonia.Visuals.Media.Imaging;
using ReactiveUI;
using Serilog;
using SkiaSharp;

namespace Centurion.Cli.AvaloniaUI.Behaviors;

public class BitmapLoader : AvaloniaObject
{
  private static readonly ILogger Logger = Log.ForContext(typeof(BitmapLoader));
  private static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name!;
  private const int MaxWidth = 150;
  private static readonly HttpClient HttpClient = new();
  private static readonly ConcurrentDictionary<Uri, Lazy<Task<IBitmap>>> PicturesCache = new();
  public static readonly Uri FallbackPictureUri = new($"avares://{AssemblyName}/Assets/loading-placeholder.png");

  public static readonly AttachedProperty<string> SourceProperty =
    AvaloniaProperty.RegisterAttached<BitmapLoader, Border, string>("Source", default!, false, BindingMode.OneWay);

  static BitmapLoader()
  {
    SourceProperty.Changed.Subscribe(_ => HandleSourceChanged(_.Sender, _.NewValue.GetValueOrDefault<string>()));
  }

  private static void HandleSourceChanged(IAvaloniaObject sender, string? source)
  {
    if (sender is not Border b)
    {
      return;
    }

    if (string.IsNullOrEmpty(source))
    {
      b.Background = new ImageBrush(ReadFromAssets(FallbackPictureUri));
      return;
    }

    b.Background = new ImageBrush(ReadFromAssets(FallbackPictureUri));

    Fetch(source)
      .ToObservable()
      .ObserveOn(RxApp.MainThreadScheduler)
      .Catch<IBitmap, Exception>(exc =>
      {
        Logger.Error(exc, "Failed to load picture");
        return Observable.Return(ReadFromAssets(FallbackPictureUri));
      })
      .Subscribe(bitmap => b.Background = new ImageBrush(bitmap));
  }

  /// <summary>
  /// Accessor for Attached property <see cref="SourceProperty"/>.
  /// </summary>
  public static void SetSource(AvaloniaObject element, string commandValue)
  {
    element.SetValue(SourceProperty, commandValue);
  }

  /// <summary>
  /// Accessor for Attached property <see cref="SourceProperty"/>.
  /// </summary>
  public static string GetSource(AvaloniaObject element)
  {
    return element.GetValue(SourceProperty);
  }


  private static IBitmap ReadFromAssets(Uri uri)
  {
    var assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!;
    return new Bitmap(assets.Open(uri));
  }

  private static async Task<IBitmap> DownloadPicture(Uri uri)
  {
    var r = await HttpClient.GetAsync(uri);
    if (!r.IsSuccessStatusCode)
    {
      return ReadFromAssets(FallbackPictureUri);
    }

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

  private static async Task<IBitmap> Fetch(string path)
  {
    if (!path.StartsWith("http") && !path.StartsWith("avares"))
    {
      path = $"avares://{AssemblyName}{path}";
    }

    var uri = new Uri(path, UriKind.RelativeOrAbsolute);

    return await PicturesCache.GetOrAdd(uri, picUri => new Lazy<Task<IBitmap>>(async () => uri.Scheme switch
    {
      "file" => new Bitmap(picUri.OriginalString),
      "http" => await DownloadPicture(picUri),
      "https" => await DownloadPicture(picUri),
      _ => ReadFromAssets(picUri)
    })).Value;
  }
}