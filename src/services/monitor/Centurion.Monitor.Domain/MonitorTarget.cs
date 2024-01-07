using Centurion.Contracts;

namespace Centurion.Monitor.Domain;

public class MonitorTarget
{
  public Guid TaskId { get; init; }
  public Module Module { get; init; }
  public string UserId { get; init; } = null!;

  public byte[] ModuleConfig { get; set; } = Array.Empty<byte>();
  public MonitorSettings Settings { get; init; } = null!;

  public string Sku { get; init; } = null!;
  public string Title { get; init; } = null!;
  public string? Picture { get; init; }
  public Uri PageUrl { get; init; } = null!;
  public IDictionary<string, string> Extra { get; set; } = new Dictionary<string, string>();
}