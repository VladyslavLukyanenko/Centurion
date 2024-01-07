namespace Centurion.Accounts.Core.WebHooks.Services;

public interface IWebHookPayloadSignatureCalculator
{
  ValueTask<string> CalculateSignature(string serializedPayload, string eventType, WebHooksConfig config,
    CancellationToken ct = default);
}