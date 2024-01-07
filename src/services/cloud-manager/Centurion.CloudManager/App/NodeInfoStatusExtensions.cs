using Centurion.CloudManager.Domain;
using DtoNodeStatus = Centurion.Contracts.CloudManager.NodeStatus;

namespace Centurion.CloudManager.App;

public static class NodeInfoStatusExtensions
{
  public static DtoNodeStatus ToNodeInfoStatus(this Node self)
  {
    if (self.Status == NodeStatus.Running && self.Stage.Status == LifetimeStatus.Active && self.Images.Any())
    {
      return DtoNodeStatus.Running;
    }

    return self.Status switch
    {
      NodeStatus.Unknown => DtoNodeStatus.NotExists,
      NodeStatus.Pending or NodeStatus.Running => DtoNodeStatus.Creating,
      NodeStatus.ShuttingDown or NodeStatus.Stopping => DtoNodeStatus.Terminating,
      NodeStatus.Stopped or NodeStatus.Terminated => DtoNodeStatus.Terminated,
      _ => throw new ArgumentOutOfRangeException(nameof(self), "Not supported node status: " + self)
    };
  }
}