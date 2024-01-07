using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Centurion.Cli.AvaloniaUI.Converters;

public class Int32ToCornerRadiusConverter : IValueConverter
{
  public static readonly IValueConverter Instance = new Int32ToCornerRadiusConverter();

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is double side)
    {
      return new CornerRadius(side / 2);
    }

    throw new ArgumentException("Value should be of Double type");
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}