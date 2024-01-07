namespace Centurion.Accounts.Foundation.Config;

public class IdentityConfig
{
  public IList<ClientConfig> Clients { get; set; } = new List<ClientConfig>();
  public TimeSpan DiscordClientLifetime { get; set; }
  public TimeSpan LicenseKeyClientLifetime { get; set; }

  public class ClientConfig
  {
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;

    public TimeSpan AccessTokenLifetime { get; set; }
    public TimeSpan RefreshTokenLifetime { get; set; }
    public string ApiSecret { get; set; } = null!;
  }
}