using Centurion.CloudManager.Domain;

namespace Centurion.CloudManager.Infra.Services.Aws;

public static class AwsNodeStatusExtensions
{
  public static NodeStatus ToAwsNodeStatus(this string status) => status switch
  {
    "pending" => NodeStatus.Pending,
    "running" => NodeStatus.Running,
    "shutting-down" => NodeStatus.ShuttingDown,
    "terminated" => NodeStatus.Terminated,
    "stopping" => NodeStatus.Stopping,
    "stopped" => NodeStatus.Stopped,
    _ => NodeStatus.Unknown,
  };
}