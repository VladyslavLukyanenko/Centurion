namespace Centurion.Accounts.Core.Primitives;

public class Slug
  : ValueObject
{
  private  Slug()
  {
  }
        
  private Slug(UriSafeString value)
    : this(value.Value)
  {
  }

  private Slug(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentNullException(nameof(value));
    }

    Value = value;
  }

  public string Value { get; private set; } = null!;

  public static Slug Create(params string[] tokens)
  {
    return new Slug(UriSafeString.Create(tokens));
  }

  protected override IEnumerable<object> GetAtomicValues()
  {
    yield return Value;
  }

  public static Slug FromSource(string source)
  {
    return new Slug(source.ToLowerInvariant());
  }

  public static Slug CreateEmpty()
  {
    return new Slug();
  }
}