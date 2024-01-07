// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Centurion.Accounts.Core.Primitives;

public abstract class Entity<TKey>
  : EventSource, IEntity<TKey>, IEventSource
{
  private int? _requestedHashCode;

  protected Entity()
  {
  }

  protected Entity(TKey id)
  {
    Id = id;
  }

  public TKey Id { get; protected set; } = default!;

  public override bool Equals(object? obj)
  {
    if (!(obj is Entity<TKey>))
    {
      return false;
    }

    if (ReferenceEquals(this, obj))
    {
      return true;
    }

    if (GetType() != obj.GetType())
    {
      return false;
    }

    var item = (Entity<TKey>) obj;

    return Id!.Equals(item.Id);
  }

  public override int GetHashCode()
  {
    // ReSharper disable NonReadonlyMemberInGetHashCode
    if (!_requestedHashCode.HasValue)
      // XOR for random distribution (http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-for-gethashcode.aspx)
    {
      _requestedHashCode = Id!.GetHashCode() ^ 31;
    }

    return _requestedHashCode.Value;
  }

  public override string ToString()
  {
    return $"{GetType().Name}({Id!.ToString()})";
  }

  public static bool operator ==(Entity<TKey>? left, Entity<TKey>? right)
  {
    if (Equals(left, null))
    {
      return Equals(right, null);
    }

    return left.Equals(right);
  }

  public static bool operator !=(Entity<TKey>? left, Entity<TKey>? right)
  {
    return !(left == right);
  }
}

public abstract class Entity
  : Entity<long>
{
  protected Entity()
  {
  }

  protected Entity(long id)
    : base(id)
  {
  }
}