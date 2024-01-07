namespace Centurion.Accounts.Core.WebHooks.Services;

public interface IWebHookPayloadMapper
{
  bool CanMap(object @event);

  ValueTask<WebHookPayloadEnvelop?> MapAsync(object @event, CancellationToken ct = default);
}