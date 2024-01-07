using System.Reflection;
using System.Threading.Tasks.Dataflow;
using Centurion.Cli.Core;
using Elastic.Apm;

#nullable disable

namespace Centurion.Cli;

// ReSharper disable once InconsistentNaming
public static class IApmAgentExtension
{
  public static async ValueTask Flush(this IApmAgent agent)
  {
    await Task.Run(async () =>
      {
        await Task.Yield();
        var payloadSenderV2Type =
          ReflectionHelper.GetInternalType("Elastic.Apm", "Elastic.Apm.Report", "PayloadSenderV2");
        if (agent.PayloadSender.GetType() == payloadSenderV2Type)
        {
          BatchBlock<object> _eventQueue = payloadSenderV2Type
            .GetField("_eventQueue", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(agent.PayloadSender) as BatchBlock<object>;

          Task processQueueItems = payloadSenderV2Type
            .GetMethod("ProcessQueueItems", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(agent.PayloadSender, new object[] {ReceiveAll(agent)}) as Task;
          await processQueueItems;
        }
      })
      .ConfigureAwait(false);
  }

  private static object[] ReceiveAll(IApmAgent agent)
  {
    var payloadSenderV2Type =
      ReflectionHelper.GetInternalType("Elastic.Apm", "Elastic.Apm.Report", "PayloadSenderV2");
    if (agent.PayloadSender.GetType() == payloadSenderV2Type)
    {
      BatchBlock<object> _eventQueue = payloadSenderV2Type
        .GetField("_eventQueue", BindingFlags.NonPublic | BindingFlags.Instance)
        .GetValue(agent.PayloadSender) as BatchBlock<object>;

      _eventQueue.TryReceiveAll(out var eventBatchToSend);
      return eventBatchToSend?.SelectMany(batch => batch).ToArray() ?? Array.Empty<object>();
    }

    return Array.Empty<object>();
  }
}