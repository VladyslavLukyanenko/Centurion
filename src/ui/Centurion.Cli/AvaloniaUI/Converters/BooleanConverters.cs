using System.Globalization;
using Avalonia.Data.Converters;

namespace Centurion.Cli.AvaloniaUI.Converters;

public static class BooleanConverters
{
  public static IMultiValueConverter AllTrue => BooleanMultiConverter.Instance;
}

public class BooleanMultiConverter : IMultiValueConverter
{
  public static readonly IMultiValueConverter Instance = new BooleanMultiConverter();

  public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
  {
    return values.OfType<bool>().All(_ => _);
  }
}