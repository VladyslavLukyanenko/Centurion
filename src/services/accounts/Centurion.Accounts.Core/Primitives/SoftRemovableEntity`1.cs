using CSharpFunctionalExtensions;
using NodaTime;

namespace Centurion.Accounts.Core.Primitives;

public abstract class SoftRemovableEntity<T> : AuditableEntity<T>, ISoftRemovable
  where T : IComparable<T>, IEquatable<T>
{
  protected SoftRemovableEntity()
  {
  }

  protected SoftRemovableEntity(T id)
    : base(id)
  {
  }
    
  public Instant RemovedAt { get; private set; } = Instant.MaxValue;

  public bool IsRemoved() => RemovedAt == Instant.MaxValue;

  public Result Remove()
  {
    if (IsRemoved())
    {
      return Result.Failure("Already removed at " + RemovedAt);
    }

    RemovedAt = SystemClock.Instance.GetCurrentInstant();
    AddDomainEvent(new EntitySoftRemoved(this));
    return Result.Success();
  }
  //
  // public Result RemoveAt(Instant removedAt)
  // {
  //   if (RemovedAt.ToUnixTimeSeconds() != Instant.MaxValue.ToUnixTimeSeconds())
  //   {
  //     return Result.Failure("Already removed at " + RemovedAt);
  //   }
  //
  //   RemovedAt = removedAt;
  //   AddDomainEvent(new EntitySoftRemoved(this));
  //   return Result.Success();
  // }
}