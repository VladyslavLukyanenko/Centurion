#pragma warning disable 8618
using Centurion.Accounts.App.Serialization.NewtonsoftJson.Converters;
using Centurion.Accounts.Core.FileStorage.FileSystem;

namespace Centurion.Accounts.App.Products.Model;

public class ProductFeatureData
{
  [UploadedFilePath] public string Icon { get; set; }
  public IBinaryData? UploadedIcon { get; set; }
  public string Title { get; set; }
  public string Desc { get; set; }
}