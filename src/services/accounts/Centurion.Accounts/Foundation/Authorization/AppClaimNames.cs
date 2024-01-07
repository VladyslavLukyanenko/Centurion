namespace Centurion.Accounts.Foundation.Authorization;

public static class AppClaimNames
{
  public const string Id = "id";
  public const string StripeCustomerId = "stripe_customer_id";

  public const string DiscordId = "discord_id";
  public const string DiscordExpiresIn = "discord_expires_at";
  public const string DiscordRefreshedAt = "discord_refreshed_at";
  public const string DiscordAccessTokenToken = "discord_access_token";
  public const string DiscordRefreshTokenToken = "discord_refresh_token";
  public const string DiscordAvatar = "avatar";
  public const string DiscordDiscriminator = "discriminator";
  public const string DiscordRoleName = "discord_role_name";

  public const string LicenseKey = "license_key";
  public const string LicenseKeyExpiry = "key_expiry";

  public const string ProductVersion = "product_version";
  public const string OwnDashboardId = "own_dashboard_id";
  public const string JoinedDashboardId = "joined_dashboard_id";
  public const string CurrentDashboardId = "curr_dashboard_id";
  public const string CurrentDashboardHostingMode = "curr_dashboard_host_mode";
  public const string CurrentDashboardDomain = "curr_dashboard_domain";
    
  public const string Permission = "permission";
  public const string RoleName = "role_name";
  public const string RoleId = "role_id";
}