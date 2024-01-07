using System.Net;
using Centurion.Contracts;
using Centurion.Monitor.Domain.Antibot;
using Centurion.Monitor.Domain.Http;
using Centurion.Monitor.Domain.Services;
using Centurion.Net.Http;
using Centurion.TaskManager;
using Elastic.Apm.Api;
using Microsoft.Extensions.DependencyInjection;

namespace Centurion.Monitor.Domain;

public class MonitorHttpClientFactory : IMonitorHttpClientFactory
{
  private readonly string _module;
  private readonly IHttpClientFactory _clientFactory;

  public MonitorHttpClientFactory(Module module, IServiceProvider sp, AntibotProtectionConfig? antibotConfig,
    IEnumerable<Uri> proxies)
  {
    _module = module.ToString();
    var sc = new ServiceCollection();

    sc.AddHttpClient(_module, client =>
      {
        client.Timeout = TimeSpan.FromSeconds(5);
        var defaultHeaders = new Dictionary<string, string>
        {
          {
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.8.431.141 Safari/537.36"
          },
          { "Accept-Encoding", "gzip, deflate, br" },
          { "Connection", "keep-alive" },
          { "sec-ch-ua", "\" Not;A Brand\";v=\"99\", \"Google Chrome\";v=\"91\", \"Chromium\";v=\"91\"" },
          { "sec-ch-ua-mobile", "?0" },
          { "sec-fetch-dest", "document" },
          { "sec-fetch-mode", "navigate" },
          { "sec-fetch-site", "same-origin" },
          { "sec-fetch-user", "?1" },
          { "upgrade-insecure-requests", "1" },
          { "accept", "*/*" },
          { "accept-encoding", "gzip, deflate, br" },
          { "accept-language", "en-US,en;q=0.9,uk;q=0.8,ru;q=0.7,fr;q=0.6" },
          { "cache-control", "no-cache" },
          {
            KnownHeader.HeaderOrderHeaderName, string.Join(",", new[]
            {
              "accept",
              "accept-encoding",
              "accept-language",
              "cache-control",
              "cookie",
              "pragma",
              "sec-ch-ua",
              "sec-ch-ua-mobile",
              "sec-fetch-dest",
              "sec-fetch-mode",
              "sec-fetch-site",
              "sec-fetch-user",
              "upgrade-insecure-requests",
              "user-agent",
            })
          },
          {
            KnownHeader.PHeaderOrderHeaderName, string.Join(",", new[]
            {
              ":authority",
              ":method",
              ":path",
              ":scheme",
            })
          }
        };

        if (BaseAddress != null)
        {
          client.BaseAddress = BaseAddress;
        }

        foreach (var header in defaultHeaders)
        {
          client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }
      })
      .ConfigurePrimaryHttpMessageHandler(() =>
      {
        var solverProvider = sp.GetRequiredService<IAntibotProtectionSolverProvider>();
        HttpMessageHandler handler = new BalancingHttpClientHandler(proxies, CreateHttpMessageHandler);
        if (antibotConfig?.IsEmpty() == false)
        {
          var provider = antibotConfig.ProtectProvider;
          var solver = solverProvider
                         .GetSolver(provider)
                       ?? throw new InvalidOperationException($"Antibot solver {provider} is not supported");

          handler = new ProtectionSolverHttpClientHandler(handler, solver, antibotConfig);
        }

        return new TracingMessageHandler(handler, sp.GetRequiredService<ITracer>());

        HttpMessageHandler CreateHttpMessageHandler(Uri? proxyUri)
        {
          var utlsHandler = UtlsHttpMessageHandler.Create(ClientHelloSpecPresets.CreateIos121ClientHelloSpec(),
            proxyUri?.ToString() /*,
          _loggerFactory.CreateLogger<UtlsHttpMessageHandler>()*/);

          utlsHandler.UseCookies = UseCookies;
          utlsHandler.CookieContainer = CookieContainer;

          return utlsHandler;
        }
      });

    _clientFactory = sc.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
  }

  public HttpClient CreateHttpClient()
  {
    return _clientFactory.CreateClient(_module);
  }

  public bool UseCookies { get; set; }
  public CookieContainer CookieContainer { get; set; } = new();
  public Uri? BaseAddress { get; set; }


  private class TracingMessageHandler : DelegatingHandler
  {
    private readonly ITracer _tracer;

    public TracingMessageHandler(HttpMessageHandler innerHandler, ITracer tracer) : base(innerHandler)
    {
      _tracer = tracer;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
      CancellationToken cancellationToken)
    {
      var uri = request.RequestUri;
      var span = _tracer.StartSpan(uri!.AbsoluteUri, "request");
      try
      {
        var r = await base.SendAsync(request, cancellationToken);
        if (span is not null)
        {
          span.Context.Http = new Elastic.Apm.Api.Http
          {
            Method = request.Method.Method,
            Url = uri?.ToString(),
            StatusCode = (int)r.StatusCode
          };
        }

        return r;
      }
      catch (Exception exc)
      {
        span?.CaptureException(exc);
        throw;
      }
      finally
      {
        span?.End();
      }
    }
  }
}