using System.Security.Cryptography;
using System.Text;
using Centurion.Accounts.Core.WebHooks;
using Centurion.Accounts.Core.WebHooks.Services;

namespace Centurion.Accounts.Infra.WebHooks.Services;

public class HmacWebHookPayloadSignatureCalculator : IWebHookPayloadSignatureCalculator
{
  public ValueTask<string> CalculateSignature(string serializedPayload, string eventType, WebHooksConfig config,
    CancellationToken ct = default)
  {
    var secret = Encoding.UTF8.GetBytes(config.ClientSecret);
    var payload = Encoding.UTF8.GetBytes(serializedPayload);

    var algorithm = new HMACSHA256(secret);
    var hash = algorithm.ComputeHash(payload);
    var signature = Convert.ToBase64String(hash);

    return ValueTask.FromResult(signature);
  }
}