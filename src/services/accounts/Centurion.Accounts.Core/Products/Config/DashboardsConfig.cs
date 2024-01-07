using Centurion.Accounts.Core.FileStorage.Config;

namespace Centurion.Accounts.Core.Products.Config;

public class DashboardsConfig
{
  public string[] BlacklistedDomains { get; set; } = null!;
  public string LocationPathSegmentRegex { get; set; } = null!;
    
    
  public FileUploadsConfig LogoUploadConfig { get; set; } = null!;
  public FileUploadsConfig ImageUploadConfig { get; set; } = null!;
  public FileUploadsConfig FeatureIconUploadConfig { get; set; } = null!;
}