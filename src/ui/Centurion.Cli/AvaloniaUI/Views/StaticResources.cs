using System.Collections.Concurrent;
using Avalonia.Svg.Skia;

namespace Centurion.Cli.AvaloniaUI.Views;

public static class StaticResources
{
  private static ConcurrentDictionary<string, SvgSource> ResourcesCache = new();

  public static SvgSource TopWaveBorder => "/Assets/Design/TopWaveBorder.svg".ToSvg();
  public static SvgSource TopWaveBorderBranch => "/Assets/Design/TopWaveBorderBranch.svg".ToSvg();
  public static SvgSource ArrowTopRight => ArrowTopRightRelPath.ToSvg();
  public static string ArrowTopRightRelPath => "/Assets/Design/ArrowTopRight.svg";


  private static SvgSource ToSvg(this string src) =>
    ResourcesCache.GetOrAdd(src, s => SvgSource.Load<SvgSource>(s.ToAvares(), null));

  private static string ToAvares(this string s) => $"avares://{typeof(App).Assembly.GetName().Name}{s}";
}