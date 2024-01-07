#pragma warning disable 8618
using Newtonsoft.Json;

namespace Centurion.Accounts.App.Model.Discord;

public class SecurityToken
{
  public SecurityToken()
  {
  }

  public SecurityToken(int expiresIn, string accessToken, string refreshToken)
  {
    ExpiresIn = expiresIn;
    AccessToken = accessToken;
    RefreshToken = refreshToken;
  }

  [JsonProperty("expires_in")] public int ExpiresIn { get; init; }
  [JsonProperty("access_token")] public string AccessToken { get; init; }
  [JsonProperty("refresh_token")] public string RefreshToken { get; init; }
}