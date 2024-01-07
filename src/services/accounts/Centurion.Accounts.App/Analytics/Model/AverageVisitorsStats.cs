namespace Centurion.Accounts.App.Analytics.Model;

public class AverageVisitorsStats
{
  public ValueDiff<int> LiveViewsCount { get; set; } = null!;
  public int DesktopsCount { get; set; }
  public int MobileCount { get; set; }
}