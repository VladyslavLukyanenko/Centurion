using System.Text;
using Centurion.Cli.Core.Domain.Discord;
using Newtonsoft.Json;

namespace Centurion.Cli.Core.Services;

public class WebHookManager : IWebHookManager
{
  private static readonly HttpClient Client = new();
  //
  // public void EnqueueWebhook(PendingRaffleTask task)
  // {
  //   var webhookObj = new DiscordWebhookBody
  //   {
  //     Embeds = new List<Embed>
  //     {
  //       Embed.CreateFrom(task, _softwareInfoProvider.CurrentSoftwareVersion)
  //     }
  //   };
  //
  //   TryEnqueue(webhookObj);
  // }

  public async Task<bool> TestWebhook(string url)
  {
    if (string.IsNullOrEmpty(url))
    {
      return false;
    }

    var webhookObj = new DiscordWebhookBody
    {
      Content = "",
      Username = "Centurion",
      AvatarUrl =
        "https://cdn.discordapp.com/attachments/733060843733909564/876326884831555615/Centurion_Social_PFP_by_iamnotsrc.png",
      Embeds = new List<Embed>
      {
        new()
        {
          Author = new Author
          {
            Name = "",
            Url = "",
            IconUrl =
              "https://cdn.discordapp.com/attachments/733060843733909564/876326884831555615/Centurion_Social_PFP_by_iamnotsrc.png"
          },
          Color = 2970110,
          Title = "**Test Webhook** :partying_face:",
          Url = "",
          Image = new Image
          {
            Url =
              "https://cdn.discordapp.com/attachments/733060843733909564/876326884831555615/Centurion_Social_PFP_by_iamnotsrc.png"
          },
          Thumbnail = new Image { Url = "" },
          Footer = new Footer
          {
            Text = $"Centurion v{AppInfo.CurrentAppVersion}",
            IconUrl =
              "https://cdn.discordapp.com/attachments/733060843733909564/876326884831555615/Centurion_Social_PFP_by_iamnotsrc.png"
          },
          Fields = new List<Field>()
        }
      }
    };

    try
    {
      var webhookContent = new StringContent(JsonConvert.SerializeObject(webhookObj), Encoding.UTF8,
        "application/json");
      var webhookResp = await Client.PostAsync(url, webhookContent);

      return (int)webhookResp.StatusCode == 204;
    }
    catch
    {
      return false;
    }
  }
}