using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Core.Products.Events.Dashboards;
using NodaTime;

namespace Centurion.Accounts.Core.Products;

public class Dashboard : SoftRemovableEntity<Guid>
{
  private bool _chargeBackersExportEnabled;

  private Dashboard()
  {
  }

  public Dashboard(long ownerId, ulong guildId, string discordAccessToken, HostingConfig hostingConfig)
  {
    OwnerId = ownerId;
    DiscordConfig.GuildId = guildId;
    DiscordConfig.BotAccessToken = discordAccessToken;
    HostingConfig = hostingConfig;
    TimeZoneId = DateTimeZone.Utc.Id /*"Etc/GMT"*/;
  }

  public ProductInfo ProductInfo { get; private set; } = new();
  public StripeIntegrationConfig StripeConfig { get; private set; } = null!;
  public long OwnerId { get; private set; }
  public Instant? ExpiresAt { get; private set; }
  public DiscordConfig DiscordConfig { get; private set; } = new();
  public string TimeZoneId { get; set; } = null!;
  public HostingConfig HostingConfig { get; private set; } = new();

  public bool ChargeBackersExportEnabled
  {
    get => _chargeBackersExportEnabled;
    set
    {
      if (value == _chargeBackersExportEnabled)
      {
        return;
      }

      _chargeBackersExportEnabled = value;
      AddDomainEvent(new ChargeBackersExportToggled(Id, value));
    }
  }
}