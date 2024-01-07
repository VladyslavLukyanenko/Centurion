using Centurion.Accounts.App.Serialization.NewtonsoftJson.Converters;

namespace Centurion.Accounts.App.Products.Model;

public class ProductPublicInfoData
{
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;
  [UploadedFilePath] public string? LogoSrc { get; set; }
  [UploadedFilePath] public string? ImageSrc { get; set; }
  public string Version { get; set; } = null!;
  public IList<ProductFeatureData> Features { get; set; } = new List<ProductFeatureData>();

}