using System.Diagnostics;
using System.Net;

namespace Centurion.Monitor.Domain.Http;

public class BalancingHttpClientHandler : HttpMessageHandler
{
  private readonly Func<Uri?, HttpMessageHandler> _handlerFactory;
  private readonly DecoratedHttpMessageHandler[] _handlers;
  private uint _currHandlerIx;
  private volatile bool _disposed;

  public BalancingHttpClientHandler(IEnumerable<Uri> proxies, Func<Uri?, HttpMessageHandler>? handlerFactory = null)
  {
    _handlerFactory = handlerFactory ?? FallbackHttpHandlerFactory;
    _handlers = proxies.Select(CreateHandler)
      .ToArray();

    // if no proxies provided
    if (_handlers.Length == 0)
    {
      _handlers = new[] {CreateHandler()};
    }
  }

  protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
  {
    if (request == null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    return GetNextHandler().SendMessage(request, cancellationToken);
  }

  protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
    CancellationToken cancellationToken)
  {
    if (request == null)
    {
      throw new ArgumentNullException(nameof(request));
    }

    return GetNextHandler().SendMessageAsync(request, cancellationToken);
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && !_disposed)
    {
      _disposed = true;
      foreach (var client in _handlers)
      {
        client.Dispose();
      }
    }

    base.Dispose(disposing);
  }

  private DecoratedHttpMessageHandler GetNextHandler()
  {
    CheckDisposed();
    var ix = Interlocked.Increment(ref _currHandlerIx) - 1; // otherwise 0 will be last item
    var handler = _handlers[ix % _handlers.Length];
    return handler;
  }

  private void CheckDisposed()
  {
    if (_disposed)
    {
      throw new ObjectDisposedException(GetType().ToString());
    }
  }


  private DecoratedHttpMessageHandler CreateHandler(Uri? proxyUrl = null)
  {
    var handler = _handlerFactory(proxyUrl);

    return new DecoratedHttpMessageHandler(handler, proxyUrl);
  }

  private HttpClientHandler FallbackHttpHandlerFactory(Uri? proxyUrl)
  {
    var proxy = CreateProxy(proxyUrl);
    return new HttpClientHandler
    {
      Proxy = proxy,
      UseProxy = proxy != null,
      DefaultProxyCredentials = proxy?.Credentials,
      AutomaticDecompression = DecompressionMethods.All,
      UseDefaultCredentials = false
    };
  }

  private IWebProxy? CreateProxy(Uri? proxyUrl)
  {
    if (proxyUrl == null)
    {
      return null;
    }

    ICredentials? credentials = null;
    if (!string.IsNullOrEmpty(proxyUrl.UserInfo))
    {
      var parts = proxyUrl.UserInfo.Split(':', StringSplitOptions.RemoveEmptyEntries);
      credentials = new NetworkCredential(parts[0], parts[1]);
    }

    return new WebProxy(proxyUrl)
    {
      Credentials = credentials
    };
  }

  private class DecoratedHttpMessageHandler : DelegatingHandler
  {
    private readonly Uri? _proxyUrl;

    public DecoratedHttpMessageHandler(HttpMessageHandler impl, Uri? proxyUrl)
      : base(impl)
    {
      _proxyUrl = proxyUrl;
    }

    internal Task<HttpResponseMessage> SendMessageAsync(HttpRequestMessage message, CancellationToken ct)
    {
      TryTraceProxyInfo();
      return SendAsync(message, ct);
    }

    internal HttpResponseMessage SendMessage(HttpRequestMessage message, CancellationToken ct)
    {
      TryTraceProxyInfo();
      return Send(message, ct);
    }

    private void TryTraceProxyInfo()
    {
      var activity = Activity.Current;
      if (activity == null)
      {
        return;
      }

      var usedProxy = _proxyUrl != null;
      activity.SetTag("http.used_proxy", usedProxy.ToString());
      if (usedProxy)
      {
        activity.SetTag("http.proxy_url", _proxyUrl!.ToString());
      }
    }
  }
}