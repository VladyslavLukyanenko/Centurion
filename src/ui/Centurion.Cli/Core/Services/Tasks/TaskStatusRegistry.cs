using Centurion.Contracts;
using DynamicData;

namespace Centurion.Cli.Core.Services.Tasks;

public class TaskStatusRegistry : ITaskStatusRegistry
{
  private readonly SourceCache<TaskStatusInfo, Guid> _statuses = new(_ => _.TaskId);

  public TaskStatusRegistry()
  {
    Statuses = _statuses.AsObservableCache();
  }

  public IObservableCache<TaskStatusInfo, Guid> Statuses { get; }

  public void SetInitial(IEnumerable<KeyValuePair<Guid, TaskStatusData>> statusChanges)
  {
    UpdateStatus(statusChanges);
  }

  public void UpdateStatus(IEnumerable<KeyValuePair<Guid, TaskStatusData>> statusChanges)
  {
    _statuses.Edit(updater =>
    {
      foreach (var (taskId, status) in statusChanges)
      {
        updater.AddOrUpdate(new TaskStatusInfo(taskId, status));
      }
    });
  }

  public void RemoveStatus(Guid taskId)
  {
    if (!_statuses.Keys.Contains(taskId))
    {
      return;
    }

    _statuses.RemoveKey(taskId);
  }
}