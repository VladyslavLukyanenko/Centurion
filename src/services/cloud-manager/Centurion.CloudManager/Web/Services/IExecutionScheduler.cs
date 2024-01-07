namespace Centurion.CloudManager.Web.Services;

public interface IExecutionScheduler
{
  void ScheduleStartNewNodes(params string[] userIds);
  void ScheduleShutdown(string nodeId);

  void ScheduleStartContainers(string nodeId);
  void ScheduleStopContainers(string nodeId);
}