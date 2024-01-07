using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.Products;

public class LicenseKeyGeneratorConfig : ValueObject
{
  private LicenseKeyGeneratorConfig(LicenseKeyFormat format, string? template = null)
  {
    Format = format;
    Template = template;
  }

  public static LicenseKeyGeneratorConfig Guid() => new(LicenseKeyFormat.Guid);
  public static LicenseKeyGeneratorConfig RandomString() => new(LicenseKeyFormat.RandomString);
  public static LicenseKeyGeneratorConfig Custom(string template) => new(LicenseKeyFormat.Custom, template);

  public LicenseKeyFormat Format { get; private set; }
  public string? Template { get; private set; }

  protected override IEnumerable<object?> GetAtomicValues()
  {
    yield return Format;
    yield return Template;
  }
}