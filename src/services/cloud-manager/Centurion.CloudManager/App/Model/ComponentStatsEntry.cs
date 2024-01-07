namespace Centurion.CloudManager.App.Model;

public class ComponentStatsEntry
{
  public string ComponentType { get; set; } = null!;
  public string ComponentName { get; set; } = null!;
  public string Stats { get; set; } = null!;
  public DateTimeOffset Timestamp { get; set; }
}