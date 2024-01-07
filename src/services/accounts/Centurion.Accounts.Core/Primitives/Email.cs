namespace Centurion.Accounts.Core.Primitives;

public class Email : ValueObject
{
  private Email()
  {
  }

  public Email(string value, bool isConfirmed)
  {
    Value = value;
    NormalizedValue = value.ToUpperInvariant();
    IsConfirmed = isConfirmed;
  }

  public string Value { get; } = null!;
  public string NormalizedValue { get; } = null!;
  public bool IsConfirmed { get; }

  public static Email CreateConfirmed(string rawEmail)
  {
    return new Email(rawEmail, true);
  }

  public static Email CreateUnconfirmed(string rawEmail)
  {
    return new Email(rawEmail, false);
  }

  public Email ToggleConfirmed(bool confirmed)
  {
    if (IsConfirmed == confirmed)
    {
      return this;
    }

    return new Email(Value, confirmed);
  }

  protected override IEnumerable<object> GetAtomicValues()
  {
    yield return Value;
    yield return NormalizedValue;
    yield return IsConfirmed;
  }
}