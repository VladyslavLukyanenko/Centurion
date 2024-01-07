namespace Centurion.TaskManager.Application.Services;

public interface ITaskManager
{
  ValueTask<IReadOnlyList<TaskActivationDetails>> ActivateTasksAsync(string userId,
    IEnumerable<ITaskActivation> activation, CancellationToken ct = default);
  //
  // ValueTask<IReadOnlyList<TaskActivationDetails>> ReactivateTasksAsync(string userId,
  //   IEnumerable<ActivatedTask> activation, CancellationToken ct = default);
  //
  // ValueTask TerminateTasksAsync(string userId, IEnumerable<Guid> taskIds, CancellationToken ct = default);
  // ValueTask TerminateTasksAsync(IEnumerable<ActivatedTask> tasks, CancellationToken ct = default);
}