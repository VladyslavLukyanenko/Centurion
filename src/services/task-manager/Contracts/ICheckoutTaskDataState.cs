using Centurion.Contracts;
using Google.Protobuf;

namespace Centurion.TaskManager.Contracts;

public interface ICheckoutTaskDataState
{
  Module Module { get; }
  string ProductSku { get; }
  ByteString Config { get; }
}