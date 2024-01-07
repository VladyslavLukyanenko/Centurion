using Centurion.Cli.Core.Clients;
using Centurion.Cli.Core.Config;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Services.Proxies;
using Centurion.Contracts;
using Centurion.Contracts.Analytics;
using Centurion.Contracts.TaskManager;
using Centurion.Contracts.Webhooks;
using Centurion.Net.Http;
using Centurion.SeedWork.Web;
using DynamicData;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Centurion.Cli.Composition;

// ReSharper disable once InconsistentNaming
public static class IServiceCollectionExtensions
{
  public static IServiceCollection AddBackendGrpcClients(this IServiceCollection sc)
  {
    var sharedGrpcOptions = new List<Action<GrpcChannelOptions>>
    {
      options => options.Credentials = ChannelCredentials.Insecure
    };

    Action<IServiceProvider, GrpcClientFactoryOptions> configureTaskmanagerClient = (svc, o) =>
    {
      var clientsConfig = svc.GetRequiredService<ClientsConfig>();
      o.Address = clientsConfig.TaskManagerUrl;
      // o.Interceptors.Add(svc.GetRequiredService<BearerTokenInterceptor>());
      o.ChannelOptionsActions.AddRange(sharedGrpcOptions);
    };

    sc.AddGrpcClient<Orchestrator.OrchestratorClient>(configureTaskmanagerClient)
      .ConfigureChannel(ConfigureChannelCreds);

    sc.AddGrpcClient<CheckoutTask.CheckoutTaskClient>(configureTaskmanagerClient)
      .ConfigureChannel(ConfigureChannelCreds);

    sc.AddGrpcClient<Products.ProductsClient>(configureTaskmanagerClient)
      .ConfigureChannel(ConfigureChannelCreds);

    sc.AddGrpcClient<Analytics.AnalyticsClient>(configureTaskmanagerClient)
      .ConfigureChannel(ConfigureChannelCreds);

    sc.AddGrpcClient<Presets.PresetsClient>(configureTaskmanagerClient)
      .ConfigureChannel(ConfigureChannelCreds);

    sc.AddGrpcClient<Webhooks.WebhooksClient>((svc, o) =>
      {
        var clientsConfig = svc.GetRequiredService<ClientsConfig>();
        o.Address = clientsConfig.WebhookServiceUrl;
        // o.Interceptors.Add(svc.GetRequiredService<BearerTokenInterceptor>());
        o.ChannelOptionsActions.AddRange(sharedGrpcOptions);
      })
      .ConfigureChannel(ConfigureChannelCreds);

    return sc;
  }

  public static IServiceCollection AddBackendHttpClients(this IServiceCollection sc)
  {
    sc.AddHttpClient<ITokenClient, TokenClient>()
      .ConfigureHttpClient((s, c) =>
      {
        var cfg = s.GetRequiredService<ClientsConfig>();
        c.BaseAddress = cfg.AccountsUrl;
      });

    sc.AddHttpClient<ICountriesApiClient, CountriesApiClient>()
      .AddHttpMessageHandler(s => s.GetRequiredService<BearerTokenHandler>())
      .ConfigureHttpClient((s, c) =>
      {
        var cfg = s.GetRequiredService<ClientsConfig>();
        c.BaseAddress = cfg.AccountsUrl;
      });

    sc.AddHttpClient<IUpdateApiClient, HttpClientUpdateApiClient>()
      .AddHttpMessageHandler(s => s.GetRequiredService<BearerTokenHandler>())
      .ConfigureHttpClient((s, c) =>
      {
        var cfg = s.GetRequiredService<ClientsConfig>();
        c.BaseAddress = cfg.AccountsUrl;
      });

    sc.AddHttpClient(Module.Amazon.ToString())
      .SetHandlerLifetime(TimeSpan.FromSeconds(10))
      .ConfigureHttpClient(client =>
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

        foreach (var header in defaultHeaders)
        {
          client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }
      })
      .ConfigurePrimaryHttpMessageHandler(s =>
      {
        var proxies = s.GetRequiredService<IProxyGroupsRepository>();
        var rnd = new Random();
        var rndProxy = proxies.LocalItems.Where(_ => _.HasAnyProxy).ToArray();
        var list = rndProxy.ElementAtOrDefault(rnd.Next(rndProxy.Length));
        var proxyUrl = list?.Proxies.ElementAtOrDefault(rnd.Next(list.Proxies.Count));

        var handler = UtlsHttpMessageHandler.Create(ClientHelloSpecPresets.CreateChrome83ClientHelloSpec(),
          proxyUrl?.ToString(), true, s.GetRequiredService<ILoggerFactory>().CreateLogger<UtlsHttpMessageHandler>());
        return handler;
        // return new DecompressionHandler(DecompressionMethods.All, handler);
      });

    sc.AddHttpClient(Module.YeezySupply.ToString())
      .SetHandlerLifetime(TimeSpan.FromSeconds(10))
      .ConfigureHttpClient(client =>
      {
        client.Timeout = TimeSpan.FromSeconds(5);

        var defaultHeaders = new Dictionary<string, string>
        {
          {
            "accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"
          },
          { "accept-encoding", "gzip, deflate, br" },
          { "accept-language", "en-US,en;q=0.9,uk;q=0.8,ru;q=0.7,fr;q=0.6" },
          { "cache-control", "no-cache" },
          { "pragma", "no-cache" },
          { "sec-ch-ua", "\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\"" },
          { "sec-ch-ua-mobile", "?0" },
          { "sec-ch-ua-platform", "\"Windows\"" },
          { "sec-fetch-dest", "document" },
          { "sec-fetch-mode", "navigate" },
          { "sec-fetch-site", "none" },
          { "sec-fetch-user", "?1" },
          { "upgrade-insecure-requests", "1" },
          {
            "user-agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.45 Safari/537.36"
          },
          {
            "cookies",
            $"UserSignUpAndSaveOverlay=0; default_searchTerms_CustomizeSearch=%5B%5D; geoRedirectionAlreadySuggested=false; wishlist=%5B%5D; persistentBasketCount=0; userBasketCount=0; UserSignUpAndSave=2; RT=\"z=1&dm=yeezysupply.com&si={Guid.NewGuid()}&ss=kwxk4r04&sl=2&tt=2ue&bcn=%2F%2F0217991b.akstat.io%2F&ul=1c6ck&hd=1c6d9\""
          },
          {
            KnownHeader.HeaderOrderHeaderName, string.Join(",", new[]
            {
              "accept",
              "accept-encoding",
              "accept-language",
              "cache-control",
              "pragma",
              "sec-ch-ua",
              "sec-ch-ua-mobile",
              "sec-ch-ua-platform",
              "sec-fetch-dest",
              "sec-fetch-mode",
              "sec-fetch-site",
              "sec-fetch-user",
              "upgrade-insecure-requests",
              "user-agent",
              "cookies",
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

        foreach (var header in defaultHeaders)
        {
          client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }
      })
      .ConfigurePrimaryHttpMessageHandler(s =>
      {
        var proxies = s.GetRequiredService<IProxyGroupsRepository>();
        var rnd = new Random();
        var rndProxy = proxies.LocalItems.Where(_ => _.HasAnyProxy).ToArray();
        var list = rndProxy.ElementAtOrDefault(rnd.Next(rndProxy.Length));
        var proxyUrl = list?.Proxies.ElementAtOrDefault(rnd.Next(list.Proxies.Count));

        var handler = UtlsHttpMessageHandler.Create(ClientHelloSpecPresets.CreateIos121ClientHelloSpec(),
          proxyUrl?.ToString(), true, s.GetRequiredService<ILoggerFactory>().CreateLogger<UtlsHttpMessageHandler>());
        return handler;
        // return new DecompressionHandler(DecompressionMethods.All, handler);
      });

    sc.AddTransient<BearerTokenHandler>();

    return sc;
  }

  private static void ConfigureChannelCreds(IServiceProvider sp, GrpcChannelOptions op)
  {
    var creds = CallCredentials.FromInterceptor((_, metadata) =>
    {
      var token = sp.GetRequiredService<ITokenProvider>();
      if (!string.IsNullOrEmpty(token.CurrentAccessToken))
      {
        metadata.Add("Authorization", $"Bearer {token.CurrentAccessToken}");
      }

      return Task.CompletedTask;
    });

    op.Credentials = ChannelCredentials.Create(new SslCredentials(), creds);
    op.MaxReceiveMessageSize = 1.Gb();
    op.MaxSendMessageSize = 1.Gb();
  }
}