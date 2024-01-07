using MassTransit;
using Centurion.Accounts.Core.Embeds.Services;
using Centurion.Accounts.Core.Services;
using Centurion.Accounts.Core.WebHooks;

namespace Centurion.Accounts.Infra.Embeds.Consumers;

public class DiscordWebHookSenderConsumer : IConsumer<WebHookPayloadEnvelop>
{
  private readonly IEmbedMessageSender _embedMessageSender;
  private readonly IEmbedRenderer _embedRenderer;
  private readonly IJsonSerializer _jsonSerializer;
  private readonly IDiscordEmbedWebHookBindingRepository _discordEmbedWebHookBindingRepository;

  public DiscordWebHookSenderConsumer(IEmbedMessageSender embedMessageSender, IEmbedRenderer embedRenderer,
    IJsonSerializer jsonSerializer, IDiscordEmbedWebHookBindingRepository discordEmbedWebHookBindingRepository)
  {
    _embedMessageSender = embedMessageSender;
    _embedRenderer = embedRenderer;
    _jsonSerializer = jsonSerializer;
    _discordEmbedWebHookBindingRepository = discordEmbedWebHookBindingRepository;
  }

  public async Task Consume(ConsumeContext<WebHookPayloadEnvelop> context)
  {
    var m = context.Message;
    if (!m.Transport.HasFlag(WebHookDeliveryTransport.DiscordWebHook))
    {
      return;
    }

    var ct = context.CancellationToken;
    var binding =
      await _discordEmbedWebHookBindingRepository.GetByEventTypeAsync(m.DashboardId, m.Payload.EventType, ct);
    if (binding == null)
    {
      return;
    }

    var renderContext =
      await _jsonSerializer.DeserializeAsync<Dictionary<string, object>>(m.Payload.Data, ct);

    var renderedData = await _embedRenderer.RenderAsync(binding, renderContext, ct);
    await _embedMessageSender.SendAsync(renderedData, binding, ct);
  }
}