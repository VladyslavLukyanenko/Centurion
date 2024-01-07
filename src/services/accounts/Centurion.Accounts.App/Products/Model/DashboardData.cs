using NodaTime;
using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.App.Products.Model;

public class DashboardData
{
  public Guid Id { get; set; }

  public ProductInfoData ProductInfo { get; set; } = null!;
  public StripeIntegrationConfig StripeConfig { get; set; } = null!;
  public Instant? ExpiresAt { get; set; }
  public DiscordConfig DiscordConfig { get; set; } = null!;
  public string TimeZoneId { get; set; } = null!;
  public HostingConfig HostingConfig { get; set; } = null!;

  public bool ChargeBackersExportEnabled { get; set; }
}