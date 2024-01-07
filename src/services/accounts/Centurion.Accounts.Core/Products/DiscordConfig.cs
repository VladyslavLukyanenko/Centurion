namespace Centurion.Accounts.Core.Products;

public class DiscordConfig
{
  public ulong GuildId { get; set; }
  public ulong RoleId { get; set; }

  /// <summary>
  /// User access token (self-bot)
  /// </summary>
  public string? AccessToken { get; set; }

  /// <summary>
  /// Bot access token. Required for Discord.Net's client and preferable way to interact with discord
  /// since user tokens officially not supported
  /// (https://support.discord.com/hc/en-us/articles/115002192352-Automated-user-accounts-self-bots-)
  /// </summary>
  public string? BotAccessToken { get; set; }

  public DiscordOAuthConfig OAuthConfig { get; set; } = new();

  public bool IsBotCredentialsEmpty() => string.IsNullOrEmpty(BotAccessToken) || GuildId == default;
  public bool IsUserCredentialsEmpty() => string.IsNullOrEmpty(AccessToken) || GuildId == default;
}