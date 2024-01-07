using Centurion.Contracts;

namespace Centurion.TaskManager.Core;

public class ActivatedTask
{
  private ActivatedTask()
  {
  }

  public ActivatedTask(Guid taskId, string userId, string sku, Module module, ISet<ProfileData> profiles,
    ProxyPoolData? checkoutProxyPool, ProxyPoolData? monitorProxyPool)
    : this(taskId, userId, sku, module, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow,
      TaskStatusData.Created, profiles, checkoutProxyPool, monitorProxyPool)
  {
  }

  public ActivatedTask(Guid taskId, string userId, string sku, Module module,
    DateTimeOffset startedAt, DateTimeOffset updatedAt, TaskStatusData status, ISet<ProfileData> profiles,
    ProxyPoolData? checkoutProxyPool, ProxyPoolData? monitorProxyPool)
  {
    TaskId = taskId;
    UserId = userId;
    Sku = sku;
    Module = module;
    StartedAt = startedAt;
    UpdatedAt = updatedAt;
    Status = status;
    Profiles = profiles;
    CheckoutProxyPool = checkoutProxyPool;
    MonitorProxyPool = monitorProxyPool;
  }

  public Guid TaskId { get; private set; }
  public string UserId { get; private set; } = null!;
  public string Sku { get; private set; } = null!;
  public Module Module { get; private set; }
  public DateTimeOffset StartedAt { get; private set; }
  public DateTimeOffset UpdatedAt { get; private set; }
  public TaskStatusData Status { get; private set; } = null!;
  public ISet<ProfileData> Profiles { get; private set; } = new HashSet<ProfileData>();
  public ProxyPoolData? CheckoutProxyPool { get; private set; }
  public ProxyPoolData? MonitorProxyPool { get; private set; }

  public bool UpdateStatusIfInProgress(TaskStatusData status)
  {
    if (Status.IsCompleted() && Status.Stage > status.Stage)
    {
      return false;
    }

    Status = status;
    UpdatedAt = DateTimeOffset.UtcNow;
    return true;
  }
}