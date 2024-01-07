using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Centurion.Cli.AvaloniaUI.Controls;

namespace Centurion.Cli.AvaloniaUI.Converters;

public class ToastColorConverter : IValueConverter
{
  private static readonly SolidColorBrush Information = new(Color.Parse("#FFFFFF"));
  private static readonly SolidColorBrush Success = new(Color.Parse("#E4C983"));
  private static readonly SolidColorBrush Warning = new(Color.Parse("#E2A74E"));
  private static readonly SolidColorBrush Error = new(Color.Parse("#E483B2"));

  public static readonly IValueConverter Instance = new ToastColorConverter();

  public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value is not NotificationType type)
    {
      throw new ArgumentException("Supported only values of type " + typeof(NotificationType), nameof(value));
    }

    return type switch
    {
      NotificationType.Info => Information,
      NotificationType.Success => Success,
      NotificationType.Warn => Warning,
      NotificationType.Error => Error,
      _ => throw new ArgumentOutOfRangeException()
    };
  }

  public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}