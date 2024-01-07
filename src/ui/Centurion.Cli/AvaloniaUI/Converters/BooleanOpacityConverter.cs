using System.Globalization;
using Avalonia.Data.Converters;

namespace Centurion.Cli.AvaloniaUI.Converters;

public class BooleanOpacityConverter : IValueConverter
{
  public static readonly IValueConverter Instance = new BooleanOpacityConverter();
  public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    return value is true ? 1D : 0D;
  }

  public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}