using System.Text.Json.Serialization;

#nullable disable
namespace Centurion.WebhookSender.Core.Discord;

public class DiscordWebhookBody
{
  [JsonPropertyName("content")] public string Content { get; set; }
  [JsonPropertyName("username")] public string Username { get; set; }
  [JsonPropertyName("avatar_url")] public string AvatarUrl { get; set; }
  [JsonPropertyName("embeds")] public List<Embed> Embeds { get; set; } = new();
}