namespace Centurion.Accounts.Core.WebHooks;

public class WebHookPayloadEnvelop
{
  public WebHookPayloadEnvelop(WebHookPayload payload, WebHooksConfig cfg, Uri? listenerEndpoint,
    WebHookDeliveryTransport transport)
  {
    Payload = payload;
    ListenerEndpoint = listenerEndpoint;
    Transport = transport;
    DashboardId = cfg.DashboardId;
  }

  public WebHookPayload Payload { get; private set; }
  public Guid DashboardId { get; private set; }
  public Uri? ListenerEndpoint { get; private set; }
  public WebHookDeliveryTransport Transport { get; private set; }
}