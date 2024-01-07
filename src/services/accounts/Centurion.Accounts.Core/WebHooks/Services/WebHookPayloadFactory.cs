using Centurion.Accounts.Core.Services;

namespace Centurion.Accounts.Core.WebHooks.Services;

public class WebHookPayloadFactory : IWebHookPayloadFactory
{
  private readonly IJsonSerializer _jsonSerializer;
  private readonly IWebHookBindingRepository _webHookBindingRepository;
  private readonly IWebHookPayloadSignatureCalculator _signatureCalculator;

  public WebHookPayloadFactory(IJsonSerializer jsonSerializer, IWebHookBindingRepository webHookBindingRepository,
    IWebHookPayloadSignatureCalculator signatureCalculator)
  {
    _jsonSerializer = jsonSerializer;
    _webHookBindingRepository = webHookBindingRepository;
    _signatureCalculator = signatureCalculator;
  }

  public async ValueTask<WebHookPayloadEnvelop> CreateAsync<T>(WebHookBinding binding, T data,
    CancellationToken ct = default)
  {
    var serializedData = await _jsonSerializer.SerializeAsync(data, ct);
    var config = await _webHookBindingRepository.GetConfigOfDashboardAsync(binding.DashboardId, ct);
    var signature = await _signatureCalculator.CalculateSignature(serializedData, binding.EventType, config, ct);

    var payload = new WebHookPayload(serializedData, signature, binding.EventType);
    return new WebHookPayloadEnvelop(payload, config, binding.ListenerEndpoint, binding.Transport);
  }
}