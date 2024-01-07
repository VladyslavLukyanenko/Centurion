using System.Collections.Concurrent;
using Centurion.Contracts.Checkout;
using Centurion.SeedWork.Web;
using Centurion.TaskManager.Core.Services;
using Grpc.Net.Client;

namespace Centurion.TaskManager.Web.Services;

public class CheckoutClientFactory : ICheckoutClientFactory
{
  private record CloudConnectionHandle(GrpcChannel Chan, Checkout.CheckoutClient Client, ICloudConnection Conn);

  private readonly ConcurrentDictionary<string, Lazy<CloudConnectionHandle>> _connections = new();

  public Checkout.CheckoutClient Create(ICloudConnection cloudConnection)
  {
    var closure = new ConnEventHandlerClosure(cloudConnection, _connections);
    var h = _connections.GetOrAdd(cloudConnection.Id, static (_, it) =>
    {
      CloudConnectionHandle CreateConnHandle()
      {
        var channel = GrpcChannel.ForAddress(it.Conn.DnsName!, new GrpcChannelOptions
        {
          HttpHandler = new SocketsHttpHandler
          {
            EnableMultipleHttp2Connections = true,
          },

          MaxReceiveMessageSize = 1.Gb(),
          MaxSendMessageSize = 1.Gb()
        });

        var client = new Checkout.CheckoutClient(channel);

        it.Conn.Closed += it.CloudConnectionOnClosed;

        return new CloudConnectionHandle(channel, client, it.Conn);
      }

      return new Lazy<CloudConnectionHandle>(CreateConnHandle);
    }, closure).Value;

    return h.Client;
  }

  private class ConnEventHandlerClosure
  {
    public ConnEventHandlerClosure(ICloudConnection conn,
      ConcurrentDictionary<string, Lazy<CloudConnectionHandle>> connections)
    {
      Conn = conn;
      Connections = connections;
    }

    public ICloudConnection Conn { get; }
    public ConcurrentDictionary<string, Lazy<CloudConnectionHandle>> Connections { get; }

    public void CloudConnectionOnClosed(object? sender, EventArgs e)
    {
      Conn.Closed -= CloudConnectionOnClosed;
      if (Connections.Remove(Conn.Id, out var handle))
      {
        handle.Value.Chan.Dispose();
      }
    }
  }
}