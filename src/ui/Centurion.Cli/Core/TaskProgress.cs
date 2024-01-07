using Centurion.Contracts;

namespace Centurion.Cli.Core;

public enum TaskProgress
{
  CheckOut,
  Decline,
  Cart,
  Error,
  Running,
  Idle
}

public static class TaskProgressExtensions
{
  public static TaskProgress ToProgress(this TaskStatusData data)
  {
    return data switch
    {
      { Category: TaskCategory.CheckedOut } => TaskProgress.CheckOut,
      { Category: TaskCategory.Declined } => TaskProgress.Decline,
      { Category: TaskCategory.Carted } => TaskProgress.Cart,
      { } p when IsError(p) => TaskProgress.Error,
      { } p when p.IsRunning() => TaskProgress.Running,
      _ => TaskProgress.Idle
    };

    bool IsError(TaskStatusData status) => status.Stage is TaskStage.Idle or TaskStage.Error
                                           && status.Category is TaskCategory.FatalError or TaskCategory.Failed;
  }
}