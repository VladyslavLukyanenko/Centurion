// ReSharper disable once CheckNamespace

using System.Net;

namespace Centurion.Contracts;

public partial class TaskStatusData
{
  public static TaskStatusData Idle { get; } = new(nameof(Idle));
  public static TaskStatusData Created { get; } = new(nameof(Created));
  public static TaskStatusData Starting { get; } = new(nameof(Starting), TaskStage.Starting);
  public static TaskStatusData Monitoring { get; } = new(nameof(Monitoring), TaskStage.Running);
  public static TaskStatusData ProductInStock { get; } = new("In Stock");

  public static TaskStatusData ProductOutOfStock { get; } =
    new(TaskCategory.Failed, "Out Of Stock", TaskStage.CheckingOut);

  public static TaskStatusData CheckoutOutOfStock { get; } =
    new(TaskCategory.FatalError, "Out Of Stock", TaskStage.CheckingOut);

  public static TaskStatusData Terminating { get; } = new(TaskCategory.Terminated, "Terminating");
  public static TaskStatusData Terminated { get; } = new(TaskCategory.Terminated, "Terminated");

  public static class Amazon
  {
    public static TaskStatusData GeneratingSession { get; } =
      new("Generating Session", TaskStage.Running);

    public static TaskStatusData Antibot { get; } =
      new(TaskCategory.Failed, "Amazon Antibot", TaskStage.Running);

    public static TaskStatusData SessionNotPresent { get; } =
      new(TaskCategory.Failed, "Session Not Present", TaskStage.Error);

    public static TaskStatusData CaptchaDetected { get; } =
      new(TaskCategory.Failed, "Captcha Detected | Rotating Proxy", TaskStage.Error);

    public static TaskStatusData ProxyBanned { get; } =
      new(TaskCategory.Failed, "Proxy Banned | Rotating Proxy", TaskStage.Error);

    public static TaskStatusData UnknownErrorGeneratingSession(HttpStatusCode statusCode) =>
      new(TaskCategory.Failed, $"Unknown Error Generating Session - {statusCode}", TaskStage.Error);

    public static TaskStatusData UnknownHttpErrorMonitor(HttpStatusCode statusCode) =>
      new(TaskCategory.Failed, $"Unknown Monitor Error - {statusCode}", TaskStage.Error);

    public static TaskStatusData UnknownErrorMonitor { get; } =
      new(TaskCategory.Failed, "Unknown Error Monitoring", TaskStage.Error);
  }

  // public bool IsNotRunning() => Category is TaskCategory.Idle || IsCompleted();
  public bool IsRunning() => Stage is not TaskStage.Unspecified && !IsCompleted();

  public bool IsCompleted() => Stage is TaskStage.Idle;

  public TaskStatusData(string title, TaskStage stage = TaskStage.Idle,
    string? desc = null)
    : this(TaskCategory.Unspecified, title, stage, desc)
  {
  }

  public TaskStatusData(TaskCategory category, string title, TaskStage stage = TaskStage.Idle,
    string? desc = null)
  {
    Category = category;
    Stage = stage;
    Description = desc ?? "";
    if (string.IsNullOrEmpty(desc))
    {
      ClearDescription();
    }

    Title = title;
  }

  public static bool operator ==(TaskStatusData? left, TaskStatusData? right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(TaskStatusData? left, TaskStatusData? right)
  {
    return !Equals(left, right);
  }

  public string AsString()
  {
    if (string.IsNullOrEmpty(Description))
    {
      return Title;
    }

    return $"{Title} {Description}".Trim();
  }
}