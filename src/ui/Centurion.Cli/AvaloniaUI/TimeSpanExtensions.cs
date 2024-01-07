namespace Centurion.Cli.AvaloniaUI;

public static class TimeSpanExtensions
{
  private const string DaysFormat = @"dd\ \d\a\y\s\ hh\:mm\:ss";
  private const string HoursFormat = @"h\:mm\:ss\ \h\o\u\r\s";
  private const string MinutesFormat = @"m\:ss\ \m\i\n\u\t\e\s";
  private const string SecondsFormat = @"ss\ \s\e\c\o\n\d\s";

  public static string ToHumanReadableString(this TimeSpan timeLeft)
  {
    if (timeLeft < TimeSpan.Zero)
    {
      return "Right now...";
    }

    string format;
    if (timeLeft < TimeSpan.FromMinutes(1))
    {
      format = SecondsFormat;
    }
    else if (timeLeft < TimeSpan.FromHours(1))
    {
      format = MinutesFormat;
    }
    else if (timeLeft < TimeSpan.FromDays(1))
    {
      format = HoursFormat;
    }
    else
    {
      format = DaysFormat;
    }

    return timeLeft.ToString(format);
  }
}