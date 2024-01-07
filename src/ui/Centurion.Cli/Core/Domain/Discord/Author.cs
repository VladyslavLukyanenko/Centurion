using System.Reflection;
using Newtonsoft.Json;

namespace Centurion.Cli.Core.Domain.Discord;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class Author
{
  [JsonProperty("name")] public string Name { get; set; } = "";

  [JsonProperty("url")] public string Url { get; set; } = "";

  [JsonProperty("icon_url")]
  public string IconUrl { get; set; } = "https://cdn.discordapp.com/attachments/733060843733909564/876326884831555615/Centurion_Social_PFP_by_iamnotsrc.png";
}