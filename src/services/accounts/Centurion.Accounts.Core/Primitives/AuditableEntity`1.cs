

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Centurion.Accounts.Core.Primitives;

public abstract class AuditableEntity<TKey>
  : TimestampAuditableEntity<TKey>, IAuthorAuditable
  where TKey : IComparable<TKey>, IEquatable<TKey>
{
  protected AuditableEntity()
  {
  }

  protected AuditableEntity(TKey id)
    : base(id)
  {
  }

  public long? UpdatedBy { get; private set; }
  public long? CreatedBy { get; private set; }
}