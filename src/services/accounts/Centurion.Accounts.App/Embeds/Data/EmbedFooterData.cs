#pragma warning disable 8618
using Newtonsoft.Json;

namespace Centurion.Accounts.App.Embeds.Data;

public class EmbedFooterData
{
  [JsonProperty("text")] public string Text { get; set; }
  [JsonProperty("icon_url")] public string IconUrl { get; set; }
}