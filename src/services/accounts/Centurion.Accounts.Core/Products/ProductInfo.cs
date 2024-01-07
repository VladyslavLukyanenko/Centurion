namespace Centurion.Accounts.Core.Products;

public class ProductInfo
{
  private const string DefaultName = "My Product";
  private const string DefaultDescription = "This is description of the product";
  private static readonly Version DefaultVersion = new(1, 0, 0);

  public string Name { get; set; } = DefaultName;
  public string Description { get; set; } = DefaultDescription;
  public Version Version { get; set; } = DefaultVersion;
  public IList<ProductFeature> Features { get; set; } = new List<ProductFeature>();
  public string? LogoSrc { get; set; }
  public string? ImageSrc { get; set; }
}