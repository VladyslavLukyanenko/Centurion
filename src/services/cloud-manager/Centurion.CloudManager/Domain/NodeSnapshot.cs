using Centurion.SeedWork.Primitives;

namespace Centurion.CloudManager.Domain;

public class NodeSnapshot : TimestampAuditableEntity
{
  private NodeSnapshot()
  {
  }

  private NodeSnapshot(UserInfo user, string providerName, string? nodeId = null)
  {
    User = user;
    ProviderName = providerName;
    NodeId = nodeId;
  }

  public static NodeSnapshot ScheduledAWS(UserInfo user) => new(user, Clouds.AWS);

  public void AttachTo(Node node) => NodeId = node.Id;

  /// <summary>
  /// If not set - it means that this is pending node, its scheduled to spawn
  /// </summary>
  public string? NodeId { get; private set; }

  public UserInfo User { get; private set; } = UserInfo.CreateEmpty();
  public string ProviderName { get; private set; } = null!;
}