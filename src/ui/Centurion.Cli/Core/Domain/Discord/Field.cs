using System.Reflection;
using Newtonsoft.Json;

namespace Centurion.Cli.Core.Domain.Discord;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class Field
{
  [JsonProperty("name")] public string Name { get; set; } = null!;

  [JsonProperty("value")] public string Value { get; set; } = null!;

  [JsonProperty("inline")] public bool Inline { get; set; }
}