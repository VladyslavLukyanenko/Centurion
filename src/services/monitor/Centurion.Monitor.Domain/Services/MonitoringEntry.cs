using NodaTime;

namespace Centurion.Monitor.Domain.Services;

public class MonitoringEntry
{
  public static MonitoringEntry Create(MonitorTarget target, bool inStock) => new()
  {
    Target = target,
    InStock = inStock
  };

  public MonitorTarget Target { get; init; } = null!;
  public bool InStock { get; set; }
  public Instant UpdatedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();

  public void UpdateInStock(bool inStock)
  {
    InStock = inStock;
    UpdatedAt = SystemClock.Instance.GetCurrentInstant();
  }
}