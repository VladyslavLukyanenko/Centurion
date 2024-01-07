using System.Reflection;
using Newtonsoft.Json;

namespace Centurion.Cli.Core.Domain.Discord;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class DiscordWebhookBody
{
  [JsonProperty("content")] public string Content { get; set; } = "";

  [JsonProperty("username")] public string Username { get; set; } = "Project Raffles";

  [JsonProperty("avatar_url")]
  public string AvatarUrl { get; set; } = "https://cdn.discordapp.com/attachments/733060843733909564/876326884831555615/Centurion_Social_PFP_by_iamnotsrc.png";

  [JsonProperty("embeds")] public List<Embed> Embeds { get; set; } = new List<Embed>();
}