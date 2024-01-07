using System.Globalization;
using Avalonia.Data.Converters;
using Humanizer;
using NodaTime;
using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;


namespace Centurion.Cli.AvaloniaUI.Converters;

public class InstantHumanizerConverter : IValueConverter
{
  public static readonly IValueConverter Instance = new InstantHumanizerConverter();

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch
  {
    Timestamp timestamp => Convert(timestamp.ToDateTimeOffset(), targetType, parameter, culture),
    DateTimeOffset dateTimeOffset => dateTimeOffset.DateTime
      .ToLocalTime()
      .Humanize(),
    DateTime dateTime => dateTime.Humanize(),
    Instant instant => instant
      .ToDateTimeUtc()
      .ToLocalTime()
      .Humanize(),
    _ => throw new InvalidOperationException()
  };

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}