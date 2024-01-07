using System.Globalization;
using Avalonia.Data.Converters;

namespace Centurion.Cli.AvaloniaUI.Converters;

public class HalfOfValueConverter : IValueConverter
{
  public static readonly IValueConverter Instance = new HalfOfValueConverter();

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return (double)value / 2;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return (double)value * 2;
  }
}