using System.Net.Mime;
using System.Runtime.Serialization;
using System.Text;
using Centurion.CloudManager.App.Model;
using GreenPipes;
using MassTransit;
using MassTransit.RabbitMqTransport.Contexts;
using MassTransit.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Centurion.CloudManager.Infra.RabbitMQ;

public class ApplicationJsonMessageDeserializer : IMessageDeserializer
{
  private static ContentType ApplicationJsonContentType => new("application/json");
  private readonly string[] _messageTypes;

  public ApplicationJsonMessageDeserializer(params Type[] supportedTypes)
  {
    _messageTypes = supportedTypes.Select(t => $"urn:message:{t.Namespace}:{t.Name}")
      .ToArray();
  }

  public void Probe(ProbeContext context)
  {
  }

  ConsumeContext IMessageDeserializer.Deserialize(ReceiveContext receiveContext)
  {
    try
    {
      var messageEncoding = GetMessageEncoding(receiveContext);

      var body = messageEncoding.GetString(receiveContext.GetBody());
      var ctx = (RabbitMqReceiveContext) receiveContext;

      var customMessage = JsonConvert.DeserializeObject<ComponentStatsEntry>(body)!;
      SendContext serviceBusSendContext =
        new BasicPublishRabbitMqSendContext<ComponentStatsEntry>(ctx.Properties, ctx.Exchange, customMessage,
          CancellationToken.None);

      // this is the default scheme, that has to match in order messages to be processed
      serviceBusSendContext.ContentType = JsonMessageSerializer.JsonContentType;
      serviceBusSendContext.SourceAddress = ctx.InputAddress;

      // sending JToken because we are using default Newtonsoft deserializer/serializer
      var messageEnv = new JsonMessageEnvelope(serviceBusSendContext, JObject.Parse(body), _messageTypes);
      return new JsonConsumeContext(JsonSerializer.CreateDefault(), receiveContext, messageEnv);
    }
    catch (JsonSerializationException ex)
    {
      throw new SerializationException(
        "A JSON serialization exception occurred while deserializing the message envelope", ex);
    }
    catch (SerializationException)
    {
      throw;
    }
    catch (Exception ex)
    {
      throw new SerializationException("An exception occurred while deserializing the message envelope", ex);
    }
  }

  static Encoding GetMessageEncoding(ReceiveContext receiveContext)
  {
    var contentEncoding = receiveContext.TransportHeaders.Get("Content-Encoding", default(string));

    return string.IsNullOrWhiteSpace(contentEncoding) ? Encoding.UTF8 : Encoding.GetEncoding(contentEncoding);
  }


  public ContentType ContentType => ApplicationJsonContentType;
}