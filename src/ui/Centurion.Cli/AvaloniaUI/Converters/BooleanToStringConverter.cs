using System.Globalization;
using Avalonia.Data.Converters;

namespace Centurion.Cli.AvaloniaUI.Converters;

public class BooleanToStringConverter : IValueConverter
{
  public static readonly BooleanToStringConverter Instance = new BooleanToStringConverter();

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return value is bool b && b ? "Yes" : "No";
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}