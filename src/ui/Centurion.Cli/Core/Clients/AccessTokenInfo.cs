using System.Text.Json.Serialization;

namespace Centurion.Cli.Core.Clients;

public class AccessTokenInfo
{
  [JsonPropertyName("access_token")] public string RawAccessToken { get; init; } = null!;
  [JsonPropertyName("expires_in")] public int ExpiresInSec { get; init; }
  [JsonPropertyName("token_type")] public string TokenType { get; init; } = null!;
  [JsonPropertyName("scope")] public string Scope { get; init; } = null!;
}