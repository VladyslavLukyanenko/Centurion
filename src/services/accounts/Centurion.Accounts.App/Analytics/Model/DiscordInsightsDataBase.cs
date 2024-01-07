using NodaTime;

namespace Centurion.Accounts.App.Analytics.Model;

public abstract class DiscordInsightsDataBase
{
  public OffsetDateTime IntervalStartTimestamp { get; set; }
}