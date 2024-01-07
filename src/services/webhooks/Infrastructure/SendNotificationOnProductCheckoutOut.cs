using System.Diagnostics;
using Centurion.Contracts.Checkout.Integration;
using Centurion.TaskManager;
using Centurion.WebhookSender.Core;
using Elastic.Apm.Api;
using MassTransit;

namespace Centurion.WebhookSender.Infrastructure;

public class SendNotificationOnProductCheckoutOut : IConsumer<ProductCheckedOut>
{
  private readonly IDiscordClient _client;
  private readonly ITracer _tracer;

  public SendNotificationOnProductCheckoutOut(IDiscordClient client, ITracer tracer)
  {
    _client = client;
    _tracer = tracer;
  }

  public async Task Consume(ConsumeContext<ProductCheckedOut> context)
  {
    var rootTx = _tracer.StartAttachedTransaction("Webhook", TraceConsts.Activities.DiscordClient);
    try
    {
      if (rootTx is not null)
      {
        rootTx.Context.User = new User
        {
          Id = context.Message.UserId
        };
      }

      var result = await _client.SendNotificationAsync(context.Message);
      if (result.IsFailure)
      {
        rootTx?.CaptureError(result.Error, TraceConsts.Activities.DiscordClient, Array.Empty<StackFrame>());
      }
    }
    catch (Exception exc)
    {
      rootTx?.CaptureException(exc);
    }
    finally
    {
      rootTx?.End();
    }
  }
}