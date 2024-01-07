using System.Net.Mime;
using System.Text.Json;
using Google.Protobuf;
using MassTransit;

namespace Centurion.WebhookSender.Infrastructure.MassTransit;

public static class Urn
{
  public static string GetMimeType<T>() => $"application/protobuf+{typeof(T).Name}";
}

public class ProtobufSerializer : IMessageSerializer
{
  public ProtobufSerializer(string contentType)
  {
    ContentType = new ContentType(contentType);
  }

  public void Serialize<TTarget>(Stream stream, SendContext<TTarget> context) where TTarget : class
  {
    switch (context.Message)
    {
      case IMessage p:
      {
        p.WriteTo(stream);
        context.ContentType = new ContentType(Urn.GetMimeType<TTarget>());
        break;
      }
      case Fault f:
      {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(f);
        stream.Write(bytes);
        context.ContentType = new ContentType("application/json");
        break;
      }
      default: throw new InvalidOperationException("Unsupported message type " + context.Message?.GetType().Name);
    }
  }

  public ContentType ContentType { get; }
}