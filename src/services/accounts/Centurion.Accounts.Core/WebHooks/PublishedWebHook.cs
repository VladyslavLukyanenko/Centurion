using Centurion.Accounts.Core.Primitives;
using NodaTime;

namespace Centurion.Accounts.Core.WebHooks;

public class PublishedWebHook : Entity
{
  private PublishedWebHook()
  {
  }

  private PublishedWebHook(WebHookPayload payload, WebHookDeliveryStatus status, string? statusDescription,
    Guid dashboardId, Uri listenerEndpoint)
  {
    Payload = payload;
    Status = status;
    StatusDescription = statusDescription;
    DashboardId = dashboardId;
    ListenerEndpoint = listenerEndpoint;
  }

  public WebHookPayload Payload { get; private set; } = null!;
  public WebHookDeliveryStatus Status { get; private set; }
  public string? StatusDescription { get; private set; }
  public Guid DashboardId { get; private set; }
  public Uri ListenerEndpoint { get; private set; } = null!;


  public Instant Timestamp { get; private set; } = SystemClock.Instance.GetCurrentInstant();

  public static PublishedWebHook Succeeded(WebHookPayloadEnvelop envelop) =>
    new(envelop.Payload, WebHookDeliveryStatus.Delivered, null, envelop.DashboardId, envelop.ListenerEndpoint!);

  public static PublishedWebHook Failure(WebHookPayloadEnvelop envelop, Exception exc) =>
    new(envelop.Payload, WebHookDeliveryStatus.Failed, exc.Message, envelop.DashboardId, envelop.ListenerEndpoint!);
}