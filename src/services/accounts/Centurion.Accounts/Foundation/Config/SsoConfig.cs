namespace Centurion.Accounts.Foundation.Config;

public class SsoConfig
{
  public string ClientId { get; set; } = null!;
  public string ClientSecret { get; set; } = null!;
  public string AuthorityUrl { get; set; } = null!;
  public bool RequireHttpsMetadata { get; set; }


  public bool ValidateAudience { get; set; }
  public bool ValidateIssuer { get; set; }
  public bool ValidateLifetime { get; set; }
  public string ValidIssuer { get; set; } = null!;
  public string ValidAudience { get; set; } = null!;
}