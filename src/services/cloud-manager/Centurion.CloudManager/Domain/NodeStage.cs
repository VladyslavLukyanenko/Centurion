using Centurion.SeedWork.Primitives;
using NodaTime;

namespace Centurion.CloudManager.Domain;

public class NodeStage : ValueObject
{
  private NodeStage()
  {
  }

  public NodeStage(LifetimeStatus status)
  {
    Status = status;
    UpdatedAt = SystemClock.Instance.GetCurrentInstant();
  }

  public LifetimeStatus Status { get; private set; }
  public Instant UpdatedAt { get; private set; }

  protected override IEnumerable<object?> GetAtomicValues()
  {
    yield return Status;
  }

  public void KeepAlive()
  {
    UpdatedAt = SystemClock.Instance.GetCurrentInstant();
  }
}