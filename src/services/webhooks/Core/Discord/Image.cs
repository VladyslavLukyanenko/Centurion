using System.Text.Json.Serialization;

namespace Centurion.WebhookSender.Core.Discord;

public class Image
{
  [JsonPropertyName("url")] public string Url { get; set; } = "";
}