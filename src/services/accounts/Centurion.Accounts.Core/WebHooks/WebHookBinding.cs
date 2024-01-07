using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.WebHooks;

public class WebHookBinding : Entity
{
  public string EventType { get; set; } = null!;
  public Uri? ListenerEndpoint { get; set; }
  public Guid DashboardId { get; set; }
  public WebHookDeliveryTransport Transport { get; set; }
}