namespace Centurion.Accounts.Core.WebHooks.Services;

public interface IWebHookPayloadFactory
{
  ValueTask<WebHookPayloadEnvelop> CreateAsync<T>(WebHookBinding binding, T data, CancellationToken ct = default);
}