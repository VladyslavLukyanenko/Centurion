using System.Text.Json.Serialization;

#nullable disable
namespace Centurion.WebhookSender.Core.Discord;

public class Author
{
  [JsonPropertyName("name")] public string Name { get; set; }
  [JsonPropertyName("url")] public string Url { get; set; }
  [JsonPropertyName("icon_url")] public string IconUrl { get; set; }
}