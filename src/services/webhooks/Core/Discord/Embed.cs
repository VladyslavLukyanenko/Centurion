using System.Text.Json.Serialization;

namespace Centurion.WebhookSender.Core.Discord;

public class Embed
{
  [JsonPropertyName("author")] public Author? Author { get; set; }
  [JsonPropertyName("color")] public long Color { get; set; } = 13742199;
  [JsonPropertyName("title")] public string Title { get; set; } = null!;
  [JsonPropertyName("url")] public string? Url { get; set; }
  [JsonPropertyName("timestamp")] public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
  [JsonPropertyName("image")] public Image? Image { get; set; }
  [JsonPropertyName("thumbnail")] public Image Thumbnail { get; set; } = new();

  [JsonPropertyName("footer")]
  public Footer Footer { get; set; } = new()
  {
    Text = "@CenturionBots",
    IconUrl = "https://cdn.discordapp.com/attachments/733060843733909564/876326884831555615/Centurion_Social_PFP_by_iamnotsrc.png"
  };

  [JsonPropertyName("fields")] public List<Field> Fields { get; set; } = new();
}