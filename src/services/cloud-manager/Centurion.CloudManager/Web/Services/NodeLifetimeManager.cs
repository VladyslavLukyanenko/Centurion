using Centurion.CloudManager.Domain;
using NodaTime;

namespace Centurion.CloudManager.Web.Services;

public class NodeLifetimeManager : INodeLifetimeManager
{
  private readonly IExecutionScheduler _scheduler;
  private readonly LifetimeConfig _lifetimeConfig;
  private readonly ILogger<NodeLifetimeManager> _logger;

  public NodeLifetimeManager(IExecutionScheduler scheduler, LifetimeConfig lifetimeConfig,
    ILogger<NodeLifetimeManager> logger)
  {
    _scheduler = scheduler;
    _lifetimeConfig = lifetimeConfig;
    _logger = logger;
  }

  public void Update(IEnumerable<Node> nodes)
  {
    foreach (var node in nodes)
    {
      if (IsOutdated(node, LifetimeStatus.Active, _lifetimeConfig.KeepAlive))
      {
        node.ChangeStage(LifetimeStatus.ConnectionLost);
        _logger.LogDebug("Connection lost with {@User} and {NodeId}. Pending termination in {Delay}", node.User,
          node.Id, _lifetimeConfig.LostConnection);
      }

      if (node.Status is not NodeStatus.Running)
      {
        continue;
      }

      if (node.Images.Count == 0 && IsMatches(node, LifetimeStatus.Active, NodeStatus.Running))
      {
        _scheduler.ScheduleStartContainers(node.Id);
      }

      else if (IsOutdated(node, LifetimeStatus.ConnectionLost, _lifetimeConfig.LostConnection))
      {
        node.ChangeStage(LifetimeStatus.PendingTermination);
        _logger.LogDebug("Pending termination of {NodeId} in {Delay}", node.Id, _lifetimeConfig.PendingTermination);
      }

      else if (IsOutdated(node, LifetimeStatus.PendingTermination, _lifetimeConfig.PendingTermination))
      {
        node.ChangeStage(LifetimeStatus.PendingShutDown);
        _logger.LogDebug("Pending shut-down {NodeId} in {Delay}", node.Id, _lifetimeConfig.PendingShutDown);
      }

      else if (node.Status is NodeStatus.Running
               && IsOutdated(node, LifetimeStatus.PendingShutDown, _lifetimeConfig.PendingShutDown))
      {
        _scheduler.ScheduleShutdown(node.Id);
      }

      else if (NeedsCleanup(node) && IsOutdated(node, LifetimeStatus.PendingTermination, _lifetimeConfig.CleanupIdle))
      {
        _scheduler.ScheduleStopContainers(node.Id);
      }
    }
  }

  private bool NeedsCleanup(Node node) => node.User is not null || node.Images.Any();

  private static bool IsOutdated(Node node, LifetimeStatus expectedStatus, Duration delay = default) =>
    node.Stage.Status == expectedStatus
    && node.Stage.UpdatedAt + delay < SystemClock.Instance.GetCurrentInstant();

  private static bool IsMatches(Node node, LifetimeStatus expectedStatus,
    NodeStatus? status = null) =>
    node.Stage.Status == expectedStatus
    && status.GetValueOrDefault(node.Status) == node.Status;
}