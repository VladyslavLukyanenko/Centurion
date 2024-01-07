using Centurion.Monitor.Domain.Antibot;

namespace Centurion.Monitor.Domain;

public class MonitorSettings
{
  // public static readonly TimeSpan DefaultDelayOnAvailable = TimeSpan.FromSeconds(10);
  public static readonly TimeSpan DefaultIterationDelay = TimeSpan.FromMilliseconds(5000);

  public decimal? MaxPrice { get; init; }

  // public Uri? PreferredProxy { get; set; }
  public IReadOnlyList<Uri> Proxies { get; init; } = Array.Empty<Uri>();
  public IReadOnlyList<string> UserAgents { get; init; } = Array.Empty<string>();

  public TimeSpan? IterationDelay { get; init; }
  // public TimeSpan? DelayOnUnavailable { get; init; }

  public AntibotProtectionConfig AntibotConfig { get; init; } = new();
}