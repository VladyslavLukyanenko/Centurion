// using MessagePack;
// using StackExchange.Redis.Extensions.Core;
//
// namespace Centurion.TaskManager.Infrastructure.MessagePack;
//
// public class CustomMessagePackSerializer : ISerializer
// {
//   private readonly MessagePackSerializerOptions _options;
//
//   public CustomMessagePackSerializer()
//     : this(MessagePackSerializer.DefaultOptions)
//   {
//   }
//
//   public CustomMessagePackSerializer(MessagePackSerializerOptions options)
//   {
//     _options = options;
//   }
//
//   public byte[] Serialize(object item)
//   {
//     return MessagePackSerializer.Serialize(item, _options);
//   }
//
//   public T Deserialize<T>(byte[] serializedObject)
//   {
//     return MessagePackSerializer.Deserialize<T>(serializedObject, _options);
//   }
// }