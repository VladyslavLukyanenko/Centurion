using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.Embeds;

public class DiscordEmbedWebHookBinding : Entity
{
  private DiscordEmbedWebHookBinding()
  {
  }

  public DiscordEmbedWebHookBinding(string eventType, Guid dashboardId, Uri webhookUrl, EmbedMessageTemplate messageTemplate)
  {
    EventType = eventType;
    DashboardId = dashboardId;
    WebhookUrl = webhookUrl;
    MessageTemplate = messageTemplate;
  }

  public string EventType { get; private set; } = null!;
  public Guid DashboardId { get; private set; }
  public Uri WebhookUrl { get; set; } = null!;
  public EmbedMessageTemplate MessageTemplate { get; set; } = null!;
}

public record EmbedMessageTemplate(string Content, string Username, string AvatarUrl, List<EmbedItem> Embeds);

public record EmbedAuthor(string Name, string Url, string IconUrl);

public record EmbedField(string Name, string Value, bool Inline);

public record EmbedFooter(string Text, string IconUrl);

public record EmbedImage(string Url);

public record EmbedItem(EmbedAuthor Author, long Color, string Title, string Url, EmbedImage Image,
  EmbedImage Thumbnail, EmbedFooter Footer, List<EmbedField> Fields);