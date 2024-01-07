using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Centurion.Cli.Core;

namespace Centurion.Cli.AvaloniaUI.Converters;

public class TaskStatusColorConverter : IValueConverter
{
  public static readonly IValueConverter Instance = new TaskStatusColorConverter();
  
  public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value is not TaskProgress p)
    {
      throw new InvalidOperationException($"Only {nameof(TaskProgress)} supported as value");
    }

    if (!Application.Current!.Resources.TryGetResource(p + "Brush", out var foundBrushResource)
        || foundBrushResource is not SolidColorBrush colorBrush)
    {
      throw new InvalidOperationException("Can't find brush for status " + p);
    }

    return colorBrush;
  }

  public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}