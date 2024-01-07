namespace Centurion.TaskManager.Core;

public class CheckoutTask : CheckoutTaskState, IUserProperty
{
  public Guid GroupId { get; init; }
  public string UserId { get; init; } = null!;
  public ISet<Guid> ProfileIds { get; init; } = new HashSet<Guid>();
  public Guid? CheckoutProxyPoolId { get; init; }
  public Guid? MonitorProxyPoolId { get; init; }
}