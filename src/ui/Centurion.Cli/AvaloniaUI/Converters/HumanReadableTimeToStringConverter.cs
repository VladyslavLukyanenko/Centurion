using System.Globalization;
using Avalonia.Data.Converters;

namespace Centurion.Cli.AvaloniaUI.Converters;

public class HumanReadableTimeToStringConverter : IValueConverter
{
  public static readonly HumanReadableTimeToStringConverter Instance = new();
    
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return ((TimeSpan) value).ToHumanReadableString();
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}