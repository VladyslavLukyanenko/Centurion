using NodaTime;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Centurion.Accounts.Core.Primitives;

public abstract class TimestampAuditableEntity<TKey>
  : Entity<TKey>, ITimestampAuditable
  where TKey : IComparable<TKey>, IEquatable<TKey>
{
  protected TimestampAuditableEntity()
  {
  }

  protected TimestampAuditableEntity(TKey id)
    : base(id)
  {
  }

  public Instant CreatedAt { get; private set; } = SystemClock.Instance.GetCurrentInstant();
  public Instant UpdatedAt { get; private set; } = SystemClock.Instance.GetCurrentInstant();
}

public abstract class TimestampAuditableEntity
  : TimestampAuditableEntity<long>
{
  protected TimestampAuditableEntity()
  {
  }

  protected TimestampAuditableEntity(long id)
    : base(id)
  {
  }
}