// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Centurion.SeedWork.Primitives;

public abstract class AuditableEntity<TKey>
  : TimestampAuditableEntity<TKey>, IAuthorAuditable<TKey>
  where TKey : IComparable<TKey>, IEquatable<TKey>
{
  protected AuditableEntity()
  {
  }

  protected AuditableEntity(TKey id)
    : base(id)
  {
  }

  public TKey? UpdatedBy { get; private set; }
  public TKey? CreatedBy { get; private set; }
}