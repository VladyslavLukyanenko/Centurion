namespace Centurion.TaskManager.Application.Services;

public class TaskManager : ITaskManager
{
  // private readonly ITaskRegistry _taskRegistry;
  private readonly ICheckoutTaskProvider _taskProvider;

  public TaskManager(ICheckoutTaskProvider taskProvider)
  {
    _taskProvider = taskProvider;
  }

  public async ValueTask<IReadOnlyList<TaskActivationDetails>> ActivateTasksAsync(string userId,
    IEnumerable<ITaskActivation> activation, CancellationToken ct = default)
  {
    var activationInfos = activation.ToDictionary(_ => _.TaskId);
    var tasks = await _taskProvider.GetMappedCheckoutTasksAsync(userId, activationInfos.Keys, ct);

    return tasks.Select(t => activationInfos[t.Task.Id].CreateActivated(t))
      .Where(activationResult => !activationResult.IsFailure)
      .Select(activationResult => activationResult.Value)
      .ToArray();
      
      
      
    // var activationInfos = activation.ToDictionary(_ => _.TaskId);
    // var inactiveTaskIds = await _taskRegistry.GetInactiveTasksAsync(activationInfos.Keys, userId, ct);
    // if (inactiveTaskIds.Count == 0)
    // {
    //   return Array.Empty<TaskActivationDetails>();
    // }
    //
    // var tasks = await _taskProvider.GetMappedCheckoutTasksAsync(userId, inactiveTaskIds, ct);
    // var activatedTasks = new List<TaskActivationDetails>(tasks.Count);
    // var taskInfo = new List<ActivatedTask>(tasks.Count);
    // foreach (var mappedTask in tasks)
    // {
    //   var info = activationInfos[mappedTask.Task.Id];
    //
    //   var activationResult = info.CreateActivated(mappedTask);
    //   if (activationResult.IsFailure)
    //   {
    //     continue;
    //   }
    //
    //   activatedTasks.Add(activationResult.Value);
    //   taskInfo.Add(activationResult.Value.ActivatedTask);
    // }
    //
    // await _taskRegistry.RegisterBatchAsync(taskInfo, userId, ct);
    //
    // return activatedTasks;
  }
}