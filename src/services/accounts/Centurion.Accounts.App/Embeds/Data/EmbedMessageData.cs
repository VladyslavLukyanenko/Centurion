#pragma warning disable 8618
using Newtonsoft.Json;

namespace Centurion.Accounts.App.Embeds.Data;

public class EmbedMessageData
{
  [JsonProperty("content")] public string Content { get; set; }
  [JsonProperty("username")] public string Username { get; set; }
  [JsonProperty("avatar_url")] public string AvatarUrl { get; set; }
  [JsonProperty("embeds")] public List<EmbedItemData> Embeds { get; set; } = new();
}