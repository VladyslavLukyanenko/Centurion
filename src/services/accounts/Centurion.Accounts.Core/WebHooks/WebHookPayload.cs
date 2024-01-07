namespace Centurion.Accounts.Core.WebHooks;

public class WebHookPayload
{
  private WebHookPayload()
  {
  }

  internal WebHookPayload(string data, string signature, string eventType)
  {
    Data = data;
    Signature = signature;
    EventType = eventType;
  }

  public static WebHookPayload CreateEmpty() => new();

  public string Data { get; private set; } = null!;
  public string Signature { get; private set; } = null!;
  public string EventType { get; private set; } = null!;
}