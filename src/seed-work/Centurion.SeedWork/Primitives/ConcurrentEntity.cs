// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Centurion.SeedWork.Primitives;

public abstract class ConcurrentEntity<T>
  : AuditableEntity<T>, IConcurrentEntity
  where T : IComparable<T>, IEquatable<T>
{
  protected ConcurrentEntity(T id)
    : base(id)
  {
  }

  public string? ConcurrencyStamp { get; protected set; }
}

public abstract class ConcurrentEntity
  : ConcurrentEntity<long>
{
  protected ConcurrentEntity()
    : base(0L)
  {
  }

  protected ConcurrentEntity(long id)
    : base(id)
  {
  }
}