namespace Centurion.Accounts.Core.Primitives;

public abstract class ValueObject
  : IEquatable<ValueObject>
{
  public bool Equals(ValueObject? other)
  {
    if (other == null)
    {
      return false;
    }

    using (IEnumerator<object?> thisValues = GetAtomicValues().GetEnumerator())
    {
      using (IEnumerator<object?> otherValues = other.GetAtomicValues().GetEnumerator())
      {
        while (thisValues.MoveNext() && otherValues.MoveNext())
        {
          if (ReferenceEquals(thisValues.Current, null) ^ ReferenceEquals(otherValues.Current, null))
          {
            return false;
          }

          if (thisValues.Current != null && !thisValues.Current.Equals(otherValues.Current))
          {
            return false;
          }
        }

        return !thisValues.MoveNext() && !otherValues.MoveNext();
      }
    }
  }

  protected static bool EqualOperator(ValueObject? left, ValueObject? right)
  {
    if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
    {
      return false;
    }

    return ReferenceEquals(left, null) || left.Equals(right);
  }

  protected static bool NotEqualOperator(ValueObject? left, ValueObject? right)
  {
    return !EqualOperator(left, right);
  }

  protected abstract IEnumerable<object?> GetAtomicValues();

  public override bool Equals(object? obj)
  {
    if (obj == null || obj.GetType() != GetType())
    {
      return false;
    }

    ValueObject other = (ValueObject) obj;
    return Equals(other);
  }

  public override int GetHashCode()
  {
    return GetAtomicValues()
      .Select(x => x != null ? x.GetHashCode() : 0)
      .Aggregate((x, y) => x ^ y);
  }

  public ValueObject GetCopy()
  {
    return (ValueObject) MemberwiseClone();
  }

  public static bool operator ==(ValueObject? left, ValueObject? right)
  {
    return EqualOperator(left, right);
  }

  public static bool operator !=(ValueObject? left, ValueObject? right)
  {
    return NotEqualOperator(left, right);
  }

  public override string ToString()
  {
    var values = GetAtomicValues().ToList();

    return $"{GetType().Name}({string.Join(", ", values.Select(token => "'" + token + "'"))})";
  }
}