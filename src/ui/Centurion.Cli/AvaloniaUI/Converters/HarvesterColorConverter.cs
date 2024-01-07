using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Centurion.Cli.Core.ViewModels.Harvesters;

namespace Centurion.Cli.AvaloniaUI.Converters;

public class HarvesterColorConverter : IValueConverter
{
  private static readonly SolidColorBrush Idle = new(Color.Parse("#DEBE6C"));
  private static readonly SolidColorBrush Initializing = new(Color.Parse("#A9B5E0"));
  private static readonly SolidColorBrush Running = new(Color.Parse("#45B26B"));

  public static readonly IValueConverter Instance = new HarvesterColorConverter();

  public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value is not HarvesterStatus status)
    {
      throw new InvalidOperationException("Invalid value provided");
    }

    return status switch
    {
      HarvesterStatus.Idle => Idle,
      HarvesterStatus.Initializing => Initializing,
      HarvesterStatus.Running => Running,
      _ => throw new ArgumentOutOfRangeException()
    };
  }

  public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}