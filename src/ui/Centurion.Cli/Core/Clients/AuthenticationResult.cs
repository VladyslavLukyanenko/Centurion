namespace Centurion.Cli.Core.Clients;

public class AuthenticationResult
{
  public bool IsSuccess { get; set; }
  public string? Message { get; set; }
  public string? Avatar { get; set; }
  public ulong DiscordId { get; set; }
  public long Discriminator { get; set; }
  public string? Email { get; set; }
  public string? UserName { get; set; }
  public string? SoftwareVersion { get; set; }
  public DateTimeOffset? ExpiresAt { get; set; }
  public AccessTokenInfo? AccessToken { get; set; }

  public static AuthenticationResult CreateUnknownError()
  {
    return new()
    {
      IsSuccess = false,
      Message = "Unknown error occurred on authentication attempt"
    };
  }
}