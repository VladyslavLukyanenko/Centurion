using Newtonsoft.Json;

namespace Centurion.Accounts.App.Embeds.Data;

public class EmbedImageData
{
  [JsonProperty("url")] public string Url { get; set; } = "";
}