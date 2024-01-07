using Centurion.Contracts.Checkout;
using Grpc.Core;

namespace Centurion.Cli.Core.Services;

internal class SynchronizedRpcMessageWriter
{
  private readonly AsyncDuplexStreamingCall<RpcMessage, RpcMessage> _writer;
  private readonly SemaphoreSlim _gates = new(1, 1);

  public SynchronizedRpcMessageWriter(AsyncDuplexStreamingCall<RpcMessage, RpcMessage> writer)
  {
    _writer = writer;
  }

  public async Task Write(RpcMessage message)
  {
    try
    {
      await _gates.WaitAsync(CancellationToken.None);
      await _writer.RequestStream.WriteAsync(message);
    }
    finally
    {
      _gates.Release();
    }
  }
}