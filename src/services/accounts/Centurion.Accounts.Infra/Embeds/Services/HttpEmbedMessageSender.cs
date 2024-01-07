using System.Text;
using CSharpFunctionalExtensions;
using Centurion.Accounts.Core.Embeds;
using Centurion.Accounts.Core.Embeds.Services;

namespace Centurion.Accounts.Infra.Embeds.Services;

public class HttpEmbedMessageSender : IEmbedMessageSender
{
  private readonly IHttpClientFactory _httpClientFactory;

  public HttpEmbedMessageSender(IHttpClientFactory httpClientFactory)
  {
    _httpClientFactory = httpClientFactory;
  }

  public async ValueTask<Result> SendAsync(string serializedPayload, DiscordEmbedWebHookBinding binding,
    CancellationToken ct = default)
  {
    var client = _httpClientFactory.CreateClient("EmbedMessageSender");
    var webhookContent = new StringContent(serializedPayload, Encoding.UTF8, "application/json");
    var webhookResp = await client.PostAsync(binding.WebhookUrl, webhookContent, ct);

    return webhookResp.IsSuccessStatusCode
      ? Result.Success()
      : Result.Failure("Failed to submit webhook: " + await webhookResp.Content.ReadAsStringAsync(ct));
  }
}