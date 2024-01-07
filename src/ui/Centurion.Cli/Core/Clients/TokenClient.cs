using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;

namespace Centurion.Cli.Core.Clients;

public class TokenClient : ITokenClient
{
  private readonly HttpClient _client;

  private delegate void AuthResultPropSetter(string claimValue, AuthenticationResult r);

  private static readonly Dictionary<string, AuthResultPropSetter> Setters = new()
  {
    {"email", (val, result) => result.Email = val},
    {"name", (val, result) => result.UserName = val},
    {"discord_id", (val, result) => result.DiscordId = ulong.Parse(val)},
    {"product_version", (val, result) => result.SoftwareVersion = val},
    {"avatar", (val, result) => result.Avatar = val},
    {"discriminator", (val, result) => result.Discriminator = long.Parse(val)},
    {"key_expiry", (val, result) =>
      {
        if (DateTimeOffset.TryParse(val, out var expiresAt))
        {
          result.ExpiresAt = expiresAt;
        }
      }
    },
  };

  public TokenClient(HttpClient client)
  {
    _client = client;
  }

  public async ValueTask<AuthenticationResult> FetchTokenAsync(string licenseKey, string hwid,
    CancellationToken ct = default)
  {
    var content = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      {"key", licenseKey},
      {"hwid", hwid},
      {"did", AppInfo.ProductId},
      {"grant_type", "license-key"},
      {"client_id", "licensekey-auth.client"},
      {"client_secret", "secret"}
    }!);
    var message = new HttpRequestMessage(HttpMethod.Post, "/connect/token")
    {
      Content = content
    };

    var r = await _client.SendAsync(message, ct);
    if (!r.IsSuccessStatusCode)
    {
      var rawError = await r.Content.ReadAsStringAsync(ct);
      return new AuthenticationResult {Message = rawError}; // todo: show correct error message
    }

    var accessToken = await r.Content.ReadFromJsonAsync<AccessTokenInfo>(cancellationToken: ct);
    if (accessToken == null)
    {
      return new AuthenticationResult {Message = "Failed to authenticate. Can't get access token."};
    }

    var result = new AuthenticationResult
    {
      AccessToken = accessToken!,
      IsSuccess = true
    };
    var jwt = new JwtSecurityToken(accessToken.RawAccessToken);
    foreach (var setterInfo in Setters)
    {
      if (jwt.Payload.TryGetValue(setterInfo.Key, out var claim))
      {
        string? claimVal = claim switch
        {
          string s => s,
          DateTime d => d.ToUniversalTime().ToString("O"),
          IEnumerable _ => null,
          _ => claim.ToString()
        };

        if (claimVal == null)
        {
          continue;
        }

        setterInfo.Value(claimVal, result);
      }
    }

    return result;
  }
}