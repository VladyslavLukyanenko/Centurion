#pragma warning disable 8618
namespace Centurion.Accounts.App.Model.Discord;

public class DiscordUser
{
  public ulong Id { get; set; }
  public string Discriminator { get; set; }
  public string Username { get; set; }
  public string? Avatar { get; set; }
  public string Locale { get; set; }
  public string Email { get; set; }
}