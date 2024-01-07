#pragma warning disable 8618
using Newtonsoft.Json;

namespace Centurion.Accounts.App.Embeds.Data;

public class EmbedItemData
{
  [JsonProperty("author")] public EmbedAuthorData Author { get; set; } = new();
  [JsonProperty("color")] public long Color { get; set; }
  [JsonProperty("title")] public string Title { get; set; }
  [JsonProperty("url")] public string Url { get; set; } = "";
  [JsonProperty("image")] public EmbedImageData Image { get; set; } = new();
  [JsonProperty("thumbnail")] public EmbedImageData Thumbnail { get; set; } = new();
  [JsonProperty("footer")] public EmbedFooterData Footer { get; set; } = new();
  [JsonProperty("fields")] public List<EmbedFieldData> Fields { get; set; } = new();
}