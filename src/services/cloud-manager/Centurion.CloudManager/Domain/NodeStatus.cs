namespace Centurion.CloudManager.Domain;

public enum NodeStatus
{
  Unknown,
  Pending,
  Running,
  Stopping,
  Stopped,
  ShuttingDown,
  Terminated
}