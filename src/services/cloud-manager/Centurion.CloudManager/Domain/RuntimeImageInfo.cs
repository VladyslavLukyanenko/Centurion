namespace Centurion.CloudManager.Domain;

public class RuntimeImageInfo
{
  private const string EnvPrefix = "gg.centurion.";
  public const string RunningStateValue = "running";

  public RuntimeImageInfo(Node node, ImageInfo imageInfo, string containerId, string status,
    string state, DateTimeOffset createdAt, IEnumerable<KeyValuePair<string, string>> environmentVariables)
  {
    ImageId = imageInfo.Id;
    Name = imageInfo.Name;
    Version = imageInfo.Version;
    Node = node;
    ContainerId = containerId;
    Status = status;
    State = state;
    CreatedAt = createdAt;
    EnvironmentVariables = new Dictionary<string, string>(environmentVariables);
  }

  public static RuntimeImageInfo CreatePending(Node node, ImageInfo info, string containerId,
    IEnumerable<KeyValuePair<string, string>> environmentVariables)
  {
    return new(node, info, containerId, "pending", "starting", DateTimeOffset.UtcNow, environmentVariables);
  }

  public static string MaskEnvForLabel(string envName) => EnvPrefix + envName;
  public static bool IsMaskedEnv(string name) => name.StartsWith(EnvPrefix);
  public static string UnmaskEnv(string name) => name[EnvPrefix.Length..];

  
  public string ImageId { get; } = null!;
  public string Name { get; } = null!;
  public string Version { get; } = null!;
  public Node Node { get; }

  public string ContainerId { get; }
  public string Status { get; }
  public string State { get; }

  public DateTimeOffset CreatedAt { get; }
  public IReadOnlyDictionary<string, string> EnvironmentVariables { get; }
}