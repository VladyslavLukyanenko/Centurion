using Centurion.CloudManager.Domain;

namespace Centurion.CloudManager.Infra.Services.Gcp;

public static class GcpNodeStatusExtensions
{
  public static NodeStatus ToGcpNodeStatus(this string status) => status switch
  {
    _ when status == "PROVISIONING" || status == "STAGING" => NodeStatus.Pending, 
    "RUNNING" => NodeStatus.Running,
    "STOPPING" => NodeStatus.Stopping,
    "TERMINATED" => NodeStatus.Stopped,
    _ => NodeStatus.Unknown,
  };
}