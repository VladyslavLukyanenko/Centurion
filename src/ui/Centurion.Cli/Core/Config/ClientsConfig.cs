namespace Centurion.Cli.Core.Config;

public class ClientsConfig
{
  public Uri TaskManagerUrl { get; set; } = null!;
  public Uri NotificationsUrl { get; set; } = null!;
  public Uri WebhookServiceUrl { get; set; } = null!;
  public Uri AccountsUrl { get; set; } = null!;
}