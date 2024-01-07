using System.Net.Mime;
using System.Runtime.Serialization;
using Google.Protobuf;
using GreenPipes;
using MassTransit;
using MassTransit.Context;
using MassTransit.Metadata;
using MassTransit.RabbitMqTransport.Contexts;
using MassTransit.Serialization;

namespace Centurion.WebhookSender.Infrastructure.MassTransit;

public class CustomMessageDeserializer<T> : IMessageDeserializer
  where T : class, IMessage<T>
{
  private readonly string[] _messageTypes;
  private readonly MessageParser<T> _parser;

  public CustomMessageDeserializer(string contentType, MessageParser<T> parser)
  {
    ContentType = new ContentType(contentType);
    _messageTypes = new[] {MessageUrn.ForTypeString<T>()};
    _parser = parser;
  }

  public void Probe(ProbeContext context)
  {
  }

  ConsumeContext IMessageDeserializer.Deserialize(ReceiveContext receiveContext)
  {
    try
    {
      T message;
      using (var body = receiveContext.GetBodyStream())
      {
        message = _parser.ParseFrom(body);
      }

      var ctx = (RabbitMqReceiveContext) receiveContext;
      SendContext serviceBusSendContext = new BasicPublishRabbitMqSendContext<T>(ctx.Properties, ctx.Exchange,
        message, CancellationToken.None);

      // this is the default scheme, that has to match in order messages to be processed
      serviceBusSendContext.ContentType = JsonMessageSerializer.JsonContentType;
      serviceBusSendContext.SourceAddress = ctx.InputAddress;

      // sending JToken because we are using default Newtonsoft deserializer/serializer
      var messageEnv = new CustomMessageEnvelope(serviceBusSendContext, message, _messageTypes);
      return new CustomConsumeContext(receiveContext, messageEnv);
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

  public ContentType ContentType { get; }

  private class CustomMessageEnvelope : MessageEnvelope
  {
    public CustomMessageEnvelope(SendContext context, object message, string[] messageTypeNames)
    {
      if (context.MessageId.HasValue)
      {
        MessageId = context.MessageId.Value.ToString();
      }

      if (context.RequestId.HasValue)
      {
        RequestId = context.RequestId.Value.ToString();
      }

      if (context.CorrelationId.HasValue)
      {
        CorrelationId = context.CorrelationId.Value.ToString();
      }

      if (context.ConversationId.HasValue)
      {
        ConversationId = context.ConversationId.Value.ToString();
      }

      if (context.InitiatorId.HasValue)
      {
        InitiatorId = context.InitiatorId.Value.ToString();
      }

      if (context.SourceAddress != null)
      {
        SourceAddress = context.SourceAddress.ToString();
      }

      if (context.DestinationAddress != null)
      {
        DestinationAddress = context.DestinationAddress.ToString();
      }

      if (context.ResponseAddress != null)
      {
        ResponseAddress = context.ResponseAddress.ToString();
      }

      if (context.FaultAddress != null)
      {
        FaultAddress = context.FaultAddress.ToString();
      }

      MessageType = messageTypeNames;

      Message = message;

      if (context.TimeToLive.HasValue)
      {
        ExpirationTime = DateTime.UtcNow + context.TimeToLive;
      }

      SentTime = context.SentTime ?? DateTime.UtcNow;

      Headers = new Dictionary<string, object>();

      foreach (KeyValuePair<string, object> header in context.Headers.GetAll())
      {
        Headers[header.Key] = header.Value;
      }

      Host = HostMetadataCache.Host;
    }

    public string? MessageId { get; }
    public string? RequestId { get; }
    public string? CorrelationId { get; }
    public string? ConversationId { get; }
    public string? InitiatorId { get; }
    public string? SourceAddress { get; }
    public string? DestinationAddress { get; }
    public string? ResponseAddress { get; }
    public string? FaultAddress { get; }
    public string[] MessageType { get; }
    public object Message { get; }
    public DateTime? ExpirationTime { get; }
    public DateTime? SentTime { get; }
    public IDictionary<string, object> Headers { get; }
    public HostInfo Host { get; }
  }


  private class CustomConsumeContext : DeserializerConsumeContext
  {
    private readonly MessageEnvelope _envelope;
    private readonly IDictionary<Type, ConsumeContext?> _messageTypes;
    private readonly string[] _supportedTypes;

    private Guid? _conversationId;
    private Guid? _correlationId;
    private Uri? _destinationAddress;
    private Uri? _faultAddress;
    private Headers? _headers;
    private Guid? _initiatorId;
    private Guid? _messageId;
    private Guid? _requestId;
    private Uri? _responseAddress;
    private Uri? _sourceAddress;

    public CustomConsumeContext(ReceiveContext receiveContext, MessageEnvelope envelope)
      : base(receiveContext)
    {
      _envelope = envelope ?? throw new ArgumentNullException(nameof(envelope));
      _supportedTypes = envelope.MessageType.ToArray();
      _messageTypes = new Dictionary<Type, ConsumeContext?>();
    }

    public override Guid? MessageId => _messageId ??= ConvertIdToGuid(_envelope.MessageId);
    public override Guid? RequestId => _requestId ??= ConvertIdToGuid(_envelope.RequestId);
    public override Guid? CorrelationId => _correlationId ??= ConvertIdToGuid(_envelope.CorrelationId);
    public override Guid? ConversationId => _conversationId ??= ConvertIdToGuid(_envelope.ConversationId);
    public override Guid? InitiatorId => _initiatorId ??= ConvertIdToGuid(_envelope.InitiatorId);
    public override DateTime? ExpirationTime => _envelope.ExpirationTime;
    public override Uri? SourceAddress => _sourceAddress ??= ConvertToUri(_envelope.SourceAddress);
    public override Uri? DestinationAddress => _destinationAddress ??= ConvertToUri(_envelope.DestinationAddress);
    public override Uri? ResponseAddress => _responseAddress ??= ConvertToUri(_envelope.ResponseAddress);
    public override Uri? FaultAddress => _faultAddress ??= ConvertToUri(_envelope.FaultAddress);
    public override DateTime? SentTime => _envelope.SentTime;

    public override Headers Headers =>
      _headers ??= _envelope.Headers != null
        ? new JsonEnvelopeHeaders(_envelope.Headers)
        : NoMessageHeaders.Instance;

    public override HostInfo Host => _envelope.Host;
    public override IEnumerable<string> SupportedMessageTypes => _supportedTypes;

    public override bool HasMessageType(Type messageType)
    {
      lock (_messageTypes)
      {
        if (_messageTypes.TryGetValue(messageType, out var existing))
        {
          return existing != null;
        }
      }

      var typeUrn = MessageUrn.ForTypeString(messageType);

      return _supportedTypes.Any(x => typeUrn.Equals(x, StringComparison.OrdinalIgnoreCase));
    }

    public override bool TryGetMessage<TTarget>(out ConsumeContext<TTarget>? message)
    {
      lock (_messageTypes)
      {
        if (_messageTypes.TryGetValue(typeof(TTarget), out var existing))
        {
          message = existing as ConsumeContext<TTarget>;
          return message != null;
        }

        var typeUrn = MessageUrn.ForTypeString<TTarget>();
        if (_envelope.Message != null
            && _supportedTypes.Any(x => typeUrn.Equals(x, StringComparison.OrdinalIgnoreCase)))
        {
          _messageTypes[typeof(TTarget)] =
            message = new MessageConsumeContext<TTarget>(this, (TTarget) _envelope.Message);
          return true;
        }

        _messageTypes[typeof(TTarget)] = message = null;
        return false;
      }
    }

    /// <summary>
    /// Converts a string identifier to a Guid, if it is actually a Guid. Can throw a FormatException
    /// if things are not right
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private static Guid? ConvertIdToGuid(string id)
    {
      if (string.IsNullOrWhiteSpace(id))
      {
        return default;
      }

      if (Guid.TryParse(id, out var messageId))
      {
        return messageId;
      }

      throw new FormatException("The Id was not a Guid: " + id);
    }

    /// <summary>
    /// Convert the string to a Uri, or return null if it is empty
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    private static Uri? ConvertToUri(string? uri)
    {
      if (string.IsNullOrWhiteSpace(uri))
      {
        return null;
      }

      return new Uri(uri);
    }
  }
}