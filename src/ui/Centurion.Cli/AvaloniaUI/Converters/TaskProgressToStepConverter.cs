using System.Globalization;
using Avalonia.Data.Converters;
using Centurion.Cli.Core;
using Centurion.Contracts;
using Type = System.Type;

namespace Centurion.Cli.AvaloniaUI.Converters;

public static class TaskProgressConverters
{
  public static IValueConverter ToStep => TaskProgressToStepConverter.Instance;
  public static IValueConverter ToPercents => TaskProgressToCompletionPercentsConverter.Instance;
}

public class TaskProgressToStepConverter : IValueConverter
{
  public static readonly IValueConverter Instance = new TaskProgressToStepConverter();
  public const int StepsCount = 4;
  
  public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value is not TaskStatusData p)
    {
      throw new InvalidOperationException($"Only {nameof(TaskStatusData)} supported as value");
    }

    if (p.Stage is TaskStage.Starting)
    {
      return 1;
    }

    return p.ToProgress() switch
    {
      TaskProgress.CheckOut => StepsCount,
      TaskProgress.Decline => StepsCount,
      TaskProgress.Cart => 3,
      TaskProgress.Error => -1,
      TaskProgress.Running => 2,
      TaskProgress.Idle => 0,
      _ => throw new ArgumentOutOfRangeException()
    };
  }

  public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}
public class TaskProgressToCompletionPercentsConverter : IValueConverter
{
  public static readonly IValueConverter Instance = new TaskProgressToCompletionPercentsConverter();
  
  public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value is not TaskStatusData p)
    {
      throw new InvalidOperationException($"Only {nameof(TaskStatusData)} supported as value");
    }

    var step = (int) TaskProgressToStepConverter.Instance.Convert(value, targetType, parameter, culture)!;
    if (step < 1)
    {
      return 0;
    }

    return step / (double) TaskProgressToStepConverter.StepsCount * 100;
  }

  public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}