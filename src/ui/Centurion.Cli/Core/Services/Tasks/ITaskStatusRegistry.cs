using Centurion.Contracts;
using DynamicData;

namespace Centurion.Cli.Core.Services.Tasks;

public record TaskStatusInfo(Guid TaskId, TaskStatusData Status);

public interface ITaskStatusRegistry
{
  IObservableCache<TaskStatusInfo, Guid> Statuses { get; }
  void SetInitial(IEnumerable<KeyValuePair<Guid, TaskStatusData>> statusChanges);
  void UpdateStatus(IEnumerable<KeyValuePair<Guid, TaskStatusData>> statusChanges);
  void RemoveStatus(Guid taskId);
}