using System.Text;
using System.Web;

namespace Centurion.Accounts.Core.Products;

public class DiscordOAuthConfig
{
  public string BuildAuthorizeUrl() =>
    $"https://discord.com/api/oauth2/authorize?client_id={ClientId}&redirect_uri={HttpUtility.UrlEncode(RedirectUrl, Encoding.UTF8)}&response_type=code&scope={HttpUtility.UrlEncode(Scope, Encoding.UTF8)}";

  // public string AuthorizeUrl { get; set; } = null!;
  public string ClientId { get; set; } = null!;
  public string ClientSecret { get; set; } = null!;
  public string RedirectUrl { get; set; } = null!;
  public string Scope { get; set; } = null!;
}