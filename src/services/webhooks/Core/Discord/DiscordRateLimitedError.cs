using System.Text.Json.Serialization;

namespace Centurion.WebhookSender.Core.Discord;

public class DiscordRateLimitedError
{
  [JsonPropertyName("retry_after")] public int RetryAfter { get; set; }
}