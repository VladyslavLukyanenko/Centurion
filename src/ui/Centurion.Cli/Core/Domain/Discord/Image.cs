using System.Reflection;
using Newtonsoft.Json;

namespace Centurion.Cli.Core.Domain.Discord;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class Image
{
  [JsonProperty("url")] public string Url { get; set; } = "";
}