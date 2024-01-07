#pragma warning disable 8618
using Newtonsoft.Json;

namespace Centurion.Accounts.App.Embeds.Data;

public class EmbedFieldData
{
  [JsonProperty("name")] public string Name { get; set; }
  [JsonProperty("value")] public string Value { get; set; }
  [JsonProperty("inline")] public bool Inline { get; set; }
}