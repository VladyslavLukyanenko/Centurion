using CSharpFunctionalExtensions;

namespace Centurion.Accounts.Core.WebHooks.Services;

public interface IWebHookSender
{
  ValueTask<Result> SendAsync(WebHookPayloadEnvelop envelop, CancellationToken ct = default);
}