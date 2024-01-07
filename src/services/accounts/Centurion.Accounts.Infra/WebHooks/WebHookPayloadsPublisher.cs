using MassTransit;
using Centurion.Accounts.Core.Events;
using Centurion.Accounts.Core.Products.Events;
using Centurion.Accounts.Core.WebHooks;
using Centurion.Accounts.Core.WebHooks.Services;

namespace Centurion.Accounts.Infra.WebHooks;

public class WebHookPayloadsPublisher : IPublishObserver, ISendObserver
{
  private readonly IEnumerable<IWebHookPayloadMapper> _webHookPayloadMappers;
  private readonly IMessageDispatcher _messageDispatcher;
  private readonly IWebHookBindingRepository _webHookBindingRepository;

  public WebHookPayloadsPublisher(IEnumerable<IWebHookPayloadMapper> webHookPayloadMappers,
    IMessageDispatcher messageDispatcher, IWebHookBindingRepository webHookBindingRepository)
  {
    _webHookPayloadMappers = webHookPayloadMappers;
    _messageDispatcher = messageDispatcher;
    _webHookBindingRepository = webHookBindingRepository;
  }

  public Task PrePublish<T>(PublishContext<T> context) where T : class => Task.CompletedTask;

  public async Task PostPublish<T>(PublishContext<T> context) where T : class
  {
    await PublishWebhookAsync(context.Message, context.CancellationToken);
  }

  public Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class => Task.CompletedTask;

  public Task PreSend<T>(SendContext<T> context) where T : class => Task.CompletedTask;

  public async Task PostSend<T>(SendContext<T> context) where T : class
  {
    await PublishWebhookAsync(context.Message, context.CancellationToken);
  }

  public Task SendFault<T>(SendContext<T> context, Exception exception) where T : class => Task.CompletedTask;

  private async ValueTask PublishWebhookAsync<T>(T message, CancellationToken ct) where T : class
  {
    if (message is not IDashboardBoundEvent de)
    {
      return;
    }

    var cfg = await _webHookBindingRepository.GetConfigOfDashboardAsync(de.DashboardId, ct);
    if (!cfg.IsEnabled)
    {
      return;
    }

    var mapper = _webHookPayloadMappers.FirstOrDefault(_ => _.CanMap(message));
    if (mapper == null)
    {
      return;
    }

    WebHookPayloadEnvelop? payload = await mapper.MapAsync(message, ct);
    if (payload == null)
    {
      return;
    }

    await _messageDispatcher.SendCommandAsync(payload, ct);
  }
}