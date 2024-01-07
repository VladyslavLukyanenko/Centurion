namespace Centurion.Accounts.App.Analytics.Model;

public class VisitorsStats
{
  public ValueDiff<int> Count { get; set; } = null!;
  public IList<VisitorsStatsItem> Data { get; set; } = new List<VisitorsStatsItem>();
}