using NodaTime;

namespace Centurion.Accounts.App.Analytics.Services;

public class DiscordAnalyticsRequest
{
  public DiscordInsightsInterval Interval { get; set; }
  public Instant StartAt { get; set; }
  public Instant EndAt { get; set; }
}