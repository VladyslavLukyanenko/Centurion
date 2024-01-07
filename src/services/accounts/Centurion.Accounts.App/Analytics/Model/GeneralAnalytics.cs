using NodaTime;

namespace Centurion.Accounts.App.Analytics.Model;

public class GeneralAnalytics
{
  public Instant AggregatedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();

  public ValueDiff<decimal> TotalRevenue { get; set; } = null!;
  public ValueDiff<int> TotalUsers { get; set; } = null!;
  public ValueDiff<int> KeysSold { get; set; } = null!;
  public ValueDiff<float> RetentionRate { get; set; } = null!;
  public VisitorsStats Visitors { get; set; } = null!;
  public AverageVisitorsStats AvgLiveViews { get; set; } = null!;

  public IList<IncomeStatsItem> Income { get; set; } = new List<IncomeStatsItem>();
}