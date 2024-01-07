using System.Reflection;
using Newtonsoft.Json;

namespace Centurion.Cli.Core.Domain.Discord;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public partial class Embed
{
  private const string SuccessTitle = "**Successful Submit** :partying_face:";
  private const int SuccessColor = 0x32CD32;

  private const string FailureTitle = "**Submit Failed** :frowning:";
  private const int FailureColor = 0xCC0000;

  private const string ExpiredTitle = "**Raffle Expired** :frowning:";
  private const int ExpiredColor = 0x999999;

  [JsonProperty("author")] public Author Author { get; set; } = new Author();

  [JsonProperty("color")] public long Color { get; set; }

  [JsonProperty("title")] public string Title { get; set; } = null!;

  [JsonProperty("url")] public string Url { get; set; } = "";

  [JsonProperty("image")] public Image Image { get; set; } = new Image();

  [JsonProperty("thumbnail")] public Image Thumbnail { get; set; } = new Image();

  [JsonProperty("footer")] public Footer Footer { get; set; } = new Footer();

  [JsonProperty("fields")] public List<Field> Fields { get; set; } = new List<Field>();
  //
  // public void Add(PendingRaffleTask task)
  // {
  //   if (task.Email != null)
  //   {
  //     Fields.Add(new Field
  //     {
  //       Name = "Email",
  //       Value = $"||{task.Email.Value}||",
  //     });
  //   }
  //
  //   Fields.Add(new Field
  //   {
  //     Name = "Product",
  //     Value = task.ProductName,
  //   });
  //
  //   Fields.Add(new Field
  //   {
  //     Name = "Profile",
  //     Value = task.Profile.ProfileName
  //   });
  //
  //   Fields.Add(new Field
  //   {
  //     Name = "Site",
  //     Value = task.ProviderName,
  //   });
  //
  //   foreach (var taskFieldValue in task.FieldValues)
  //   {
  //     Fields.Add(new Field
  //     {
  //       Name = taskFieldValue.Key,
  //       Value = taskFieldValue.Value,
  //     });
  //   }
  // }
  //
  // public static Embed CreateFrom(PendingRaffleTask task, string softwareVersion)
  // {
  //   int color;
  //   string title;
  //   if (task.IsWinner == true)
  //   {
  //     color = SuccessColor;
  //     title = SuccessTitle;
  //   }
  //   else
  //   {
  //     color = task.IsExpired() ? ExpiredColor : FailureColor;
  //     title = task.IsExpired() ? ExpiredTitle : FailureTitle;
  //   }
  //
  //   var embed = new Embed
  //   {
  //     Color = color,
  //     Title = title,
  //     Footer =
  //     {
  //       Text = $"Project Raffles v{softwareVersion}"
  //     },
  //     // Thumbnail = new Image {Url = thumbnailUrl}
  //   };
  //
  //   embed.Add(task);
  //
  //   return embed;
  // }
}