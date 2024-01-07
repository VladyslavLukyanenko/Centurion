using System.Globalization;
using Avalonia.Data.Converters;

namespace Centurion.Cli.AvaloniaUI.Converters;

public static class NumberConverters
{
  public static IValueConverter Int32ToString => Int32ToStringConverter.Instance;
}

public class Int32ToStringConverter : IValueConverter
{
  public static readonly IValueConverter Instance = new Int32ToStringConverter();
  
  public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    return value?.ToString();
  }

  public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    return int.Parse(value!.ToString()!, CultureInfo.InvariantCulture);
  }
}