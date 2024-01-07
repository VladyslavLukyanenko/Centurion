using Centurion.Accounts.App.Serialization.NewtonsoftJson.Converters;
using Centurion.Accounts.Core.FileStorage.FileSystem;

namespace Centurion.Accounts.App.Products.Model;

public class ProductInfoData
{
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;

  [UploadedFilePath] public string? LogoSrc { get; set; }
  [UploadedFilePath] public string? ImageSrc { get; set; }

  public IBinaryData? UploadedLogo { get; set; }
  public IBinaryData? UploadedImage { get; set; }

  public string Version { get; set; } = null!;
  public IList<ProductFeatureData> Features { get; set; } = new List<ProductFeatureData>();
}