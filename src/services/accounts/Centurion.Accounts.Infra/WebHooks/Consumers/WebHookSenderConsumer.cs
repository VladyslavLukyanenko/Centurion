using MassTransit;
using Centurion.Accounts.Core.WebHooks;
using Centurion.Accounts.Core.WebHooks.Services;

namespace Centurion.Accounts.Infra.WebHooks.Consumers;

public class WebHookSenderConsumer : IConsumer<WebHookPayloadEnvelop>
{
  private readonly IWebHookSender _webHookSender;

  public WebHookSenderConsumer(IWebHookSender webHookSender)
  {
    _webHookSender = webHookSender;
  }

  public async Task Consume(ConsumeContext<WebHookPayloadEnvelop> context)
  {
    if (!context.Message.Transport.HasFlag(WebHookDeliveryTransport.RemoteEndpoint))
    {
      return; 
    }

    await _webHookSender.SendAsync(context.Message, context.CancellationToken);
  }
}