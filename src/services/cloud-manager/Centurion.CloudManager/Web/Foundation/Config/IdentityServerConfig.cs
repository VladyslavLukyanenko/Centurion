namespace Centurion.CloudManager.Web.Foundation.Config
{
  public class IdentityServerConfig
  {
    public string ValidAudience { get; set; } = null!;
    public string ValidIssuer { get; set; } = null!;
    public string AuthorityUrl { get; set; } = null!;
    
    public bool RequireHttpsMetadata { get; set; }
    public bool ValidateAudience { get; set; }
    public bool ValidateIssuer { get; set; }
    public bool ValidateLifetime { get; set; }
  }
}