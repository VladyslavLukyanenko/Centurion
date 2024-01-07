using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Centurion.Contracts;
using Centurion.Contracts.Checkout.Amazon;
using Centurion.Contracts.Monitor.Integration;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;
using Centurion.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Centurion.Monitor.App.Sites.Amazon;

[Monitor(SupportedSites.Amazon)]
public class AmazonMonitor : IStoreMonitor
{
  private readonly IServiceScope _serviceScope;
  private bool _isDisposed;
  private string _sessionId = null!;
  private IMonitorHttpClientFactory _clientFactory = null!;
  private AmazonMonitorFastConfig _monitorConfig = null!;
  private AmazonConfig _siteConfig = null!;

  public AmazonMonitor(IServiceScopeFactory scopeFactory)
  {
    _serviceScope = scopeFactory.CreateScope();
  }

  public async IAsyncEnumerable<MonitoringStatusChanged> Monitor(MonitorTarget target,
    [EnumeratorCancellation] CancellationToken ct)
  {
    _clientFactory.BaseAddress = _siteConfig.BaseUrl;
    await foreach (var p in MonitorAvailability(target, ct))
    {
      yield return p;
    }
  }

  private async IAsyncEnumerable<MonitoringStatusChanged> MonitorAvailability(MonitorTarget target, CancellationToken ct)
  {
    var rnd = new Random();
    var headersOrder = new[]
    {
      "rtt",
      "downlink",
      "ect",
      "sec-ch-ua",
      "sec-ch-ua-mobile",
      "upgrade-insecure-requests",
      "origin",
      "content-type",
      "user-agent",
      "accept",
      "sec-fetch-site",
      "sec-fetch-mode",
      "sec-fetch-user",
      "sec-fetch-dest",
      "referer",
      "accept-encoding",
      "accept-language",
    };

    while (true)
    {
      var client = _clientFactory.CreateHttpClient();

      rnd.Shuffle(headersOrder);
      var payloadValue = new Dictionary<string, string>
      {
        { "session-id", _sessionId },
        { "qid", "" },
        { "sr", "" },
        { "signInToHUC", "0" },
        { $"metric-asin.{target.Sku}", "1" },
        { "registryItemID.1", "" },
        { "itemCount", "1" },
        { "offeringID.1", _monitorConfig.OfferId },
        { "quantity.1", "1" },
        { "isAddon", "1" },
        { "submit.addToCart", "Submit" },
          
        //
        // { "clientName", "MoreBuyingChoices" },
        // { "verificationSessionID", _sessionId },
        // { "ASIN", target.Sku },
        // { "offerListingID", _monitorConfig.OfferId },
        // { "quantity", "1" },
        // { "registryID", "" },
        // { "registryItemID", "" },
      };

      var payload = new FormUrlEncodedContent(payloadValue!);
      var message = new HttpRequestMessage(HttpMethod.Post, "/gp/add-to-cart/html/ref=dp_ebb_0")
      {
        Content = payload,
        Headers =
        {
          { "rtt", "0" },
          { "downlink", "10" },
          { "ect", "4g" },
          { "sec-ch-ua", _siteConfig.SecUa },
          { "sec-ch-ua-mobile", "?0" },
          { "upgrade-insecure-requests", "1" },
          { "origin", _siteConfig.BaseUrl.ToString() },
          { "user-agent", _siteConfig.Ua },
          {
            "accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"
          },
          { "sec-fetch-site", "same-origin" },
          { "sec-fetch-mode", "navigate" },
          { "sec-fetch-user", "?1" },
          { "sec-fetch-dest", "document" },
          { "referer", _siteConfig.BaseUrl.ToString() },
          { "accept-encoding", "gzip, deflate, br" },
          { "accept-language", "en-US,en;q=0.9" },

          { KnownHeader.HeaderOrderHeaderName, string.Join(",", headersOrder) }
        }
      };

      var response = await client.SendAsync(message, ct);
      if (response.StatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.Forbidden)
      {
        yield return MonitoringStatusChanged.ProxyBanned(target);
        continue;
      }

      if (!response.IsSuccessStatusCode)
      {
        yield return MonitoringStatusChanged.UnknownHttpErrorMonitor(target,
          response.StatusCode);
        continue;
      }

      var rawBody = await response.Content.ReadAsStringAsync(ct);
      if (rawBody.Contains("/errors/validateCaptcha"))
      {
        yield return MonitoringStatusChanged.CaptchaDetected(target);
        continue;
      }

      if (!rawBody.Contains("Added to Cart"))
      {
        yield return MonitoringStatusChanged.OutOfStock(target);
        continue;
      }

      yield return MonitoringStatusChanged.InStock(target);
      yield break;
    }
  }

  private async IAsyncEnumerable<MonitoringStatusChanged> MonitorFrontend(MonitorTarget target, CancellationToken ct)
  {
    var rnd = new Random();
    var headersOrder = new[]
    {
      "rtt",
      "downlink",
      "ect",
      "sec-ch-ua",
      "sec-ch-ua-mobile",
      "upgrade-insecure-requests",
      "origin",
      "content-type",
      "user-agent",
      "accept",
      "sec-fetch-site",
      "sec-fetch-mode",
      "sec-fetch-user",
      "sec-fetch-dest",
      "referer",
      "accept-encoding",
      "accept-language",
    };

    while (true)
    {
      var client = _clientFactory.CreateHttpClient();

      rnd.Shuffle(headersOrder);
      var payloadValue = new Dictionary<string, string>
      {
        { "session-id", _sessionId },
        { "qid", "" },
        { "sr", "" },
        { "signInToHUC", "0" },
        { $"metric-asin.{target.Sku}", "1" },
        { "registryItemID.1", "" },
        { "itemCount", "1" },
        { "offeringID.1", _monitorConfig.OfferId },
        { "quantity.1", "1" },
        { "isAddon", "1" },
        { "submit.addToCart", "Submit" },
          
        //
        // { "clientName", "MoreBuyingChoices" },
        // { "verificationSessionID", _sessionId },
        // { "ASIN", target.Sku },
        // { "offerListingID", _monitorConfig.OfferId },
        // { "quantity", "1" },
        // { "registryID", "" },
        // { "registryItemID", "" },
      };

      var payload = new FormUrlEncodedContent(payloadValue!);
      var message = new HttpRequestMessage(HttpMethod.Post, "/gp/add-to-cart/html/ref=dp_ebb_0")
      {
        Content = payload,
        Headers =
        {
          { "rtt", "0" },
          { "downlink", "10" },
          { "ect", "4g" },
          { "sec-ch-ua", _siteConfig.SecUa },
          { "sec-ch-ua-mobile", "?0" },
          { "upgrade-insecure-requests", "1" },
          { "origin", _siteConfig.BaseUrl.ToString() },
          { "user-agent", _siteConfig.Ua },
          {
            "accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"
          },
          { "sec-fetch-site", "same-origin" },
          { "sec-fetch-mode", "navigate" },
          { "sec-fetch-user", "?1" },
          { "sec-fetch-dest", "document" },
          { "referer", _siteConfig.BaseUrl.ToString() },
          { "accept-encoding", "gzip, deflate, br" },
          { "accept-language", "en-US,en;q=0.9" },

          { KnownHeader.HeaderOrderHeaderName, string.Join(",", headersOrder) }
        }
      };

      var response = await client.SendAsync(message, ct);
      if (response.StatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.Forbidden)
      {
        yield return MonitoringStatusChanged.ProxyBanned(target);
        continue;
      }

      if (!response.IsSuccessStatusCode)
      {
        yield return MonitoringStatusChanged.UnknownHttpErrorMonitor(target,
          response.StatusCode);
        continue;
      }

      var rawBody = await response.Content.ReadAsStringAsync(ct);
      if (rawBody.Contains("/errors/validateCaptcha"))
      {
        yield return MonitoringStatusChanged.CaptchaDetected(target);
        continue;
      }

      if (!rawBody.Contains("Added to Cart"))
      {
        yield return MonitoringStatusChanged.OutOfStock(target);
        continue;
      }

      yield return MonitoringStatusChanged.InStock(target);
      yield break;
    }
  }

  private async IAsyncEnumerable<MonitoringStatusChanged> MonitorFast(MonitorTarget target, CancellationToken ct)
  {
    var rnd = new Random();
    var headersOrder = new[]
    {
      "rtt",
      "downlink",
      "ect",
      "sec-ch-ua",
      "sec-ch-ua-mobile",
      "upgrade-insecure-requests",
      "origin",
      "content-type",
      "user-agent",
      "accept",
      "sec-fetch-site",
      "sec-fetch-mode",
      "sec-fetch-user",
      "sec-fetch-dest",
      "referer",
      "accept-encoding",
      "accept-language"
    };

    while (true)
    {
      // _clientFactory.CookieContainer = new CookieContainer();
      // _clientFactory.UseCookies = true;
      var client = _clientFactory.CreateHttpClient();

      rnd.Shuffle(headersOrder);
      var payloadValue = new Dictionary<string, string>
      {
        { "session-id", _sessionId },
        { "clientName", "retailwebsite" },
        { "nextPage", "cartitems" },
        { "ASIN", target.Sku },
        { "offerListingID", _monitorConfig.OfferId },
        { "quantity", "1" },
      };

      var payload = new FormUrlEncodedContent(payloadValue!);

      var message = new HttpRequestMessage(HttpMethod.Post, "/gp/add-to-cart/json")
      {
        Content = payload,
        Headers =
        {
          { "rtt", "0" },
          { "downlink", "10" },
          { "ect", "4g" },
          { "sec-ch-ua", _siteConfig.SecUa },
          { "sec-ch-ua-mobile", "?0" },
          { "upgrade-insecure-requests", "1" },
          { "origin", _siteConfig.BaseUrl.ToString() },
          { "user-agent", _siteConfig.Ua },
          {
            "accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"
          },
          { "sec-fetch-site", "same-origin" },
          { "sec-fetch-mode", "navigate" },
          { "sec-fetch-user", "?1" },
          { "sec-fetch-dest", "document" },
          { "referer", _siteConfig.BaseUrl.ToString() },
          { "accept-encoding", "gzip, deflate, br" },
          { "accept-language", "en-US,en;q=0.9" },
          { KnownHeader.HeaderOrderHeaderName, string.Join(",", headersOrder) }
        }
      };

      var response = await client.SendAsync(message, ct);
      if (response.StatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.Forbidden)
      {
        yield return MonitoringStatusChanged.ProxyBanned(target);
        continue;
      }

      if (!response.IsSuccessStatusCode)
      {
        yield return MonitoringStatusChanged.UnknownHttpErrorMonitor(target,
          response.StatusCode);
        continue;
      }

      var cartInfoRaw = await response.Content.ReadAsStringAsync(ct);
      if (cartInfoRaw.Contains("/errors/validateCaptcha"))
      {
        yield return MonitoringStatusChanged.CaptchaDetected(target);
        continue;
      }

      var info = JsonSerializer.Deserialize<CartInfo>(cartInfoRaw);
      if (info == null || !info.IsOk)
      {
        yield return MonitoringStatusChanged.UnknownErrorMonitor(target);
        continue;
      }

      if (info.Items.Count == 0)
      {
        yield return MonitoringStatusChanged.OutOfStock(target);
        continue;
      }

      yield return MonitoringStatusChanged.InStock(target);
      yield break;
    }
  }

  public bool IsInitialized => IsSessionValid(_sessionId);

  public async IAsyncEnumerable<MonitoringStatusChanged> Initialize(MonitorTarget target,
    [EnumeratorCancellation] CancellationToken ct)
  {
    while (!IsInitialized)
    {
      _clientFactory = new MonitorHttpClientFactory(target.Module, _serviceScope.ServiceProvider,
        target.Settings.AntibotConfig, target.Settings.Proxies);

      _siteConfig = AmazonConfig.Parser.ParseFrom(target.ModuleConfig);

      // todo: make it OK
      if (string.IsNullOrEmpty(_siteConfig.Ua))
      {
        _siteConfig.Ua =
          "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";
      }

      if (string.IsNullOrEmpty(_siteConfig.SecUa))
      {
        _siteConfig.SecUa = "\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"90\", \"Google Chrome\";v=\"90\"";
      }

      // todo: enable it when we come back to amazon
      // _monitorConfig = _siteConfig.ModeCase == AmazonConfig.ModeOneofCase.Checkout
      //   ? _siteConfig.Checkout.MonitorConfig
      //   : _siteConfig.Turbo.MonitorConfig;

      Uri domainUrl;
      var cookieContainer = new CookieContainer();
      _clientFactory.CookieContainer = cookieContainer;
      _clientFactory.UseCookies = true;
      var client = _clientFactory.CreateHttpClient();
      if (target.Extra.TryGetValue("cookie", out var cookie))
      {
        domainUrl = _siteConfig.BaseUrl;
        foreach (var c in cookie.Split(";", StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()))
        {
          cookieContainer.SetCookies(_siteConfig.BaseUrl, c);
        }
      }
      else
      {
        domainUrl = _siteConfig.GetRandomDomainUrl();
        var message = CreateGenerateSessionRequest(domainUrl);
        var response = await client.SendAsync(message, ct);
        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
          yield return MonitoringStatusChanged.Antibot(target);
        }

        if (!response.IsSuccessStatusCode)
        {
          yield return MonitoringStatusChanged.UnknownErrorGeneratingSession(target, response.StatusCode);
          continue;
        }

        var body = await response.Content.ReadAsStringAsync(ct);
        if (body.Contains("/errors/validateCaptcha", StringComparison.OrdinalIgnoreCase))
        {
          yield return MonitoringStatusChanged.CaptchaDetected(target);
          continue;
        }
      }

      var cookies = cookieContainer.GetCookies(domainUrl);
      var sessionIdCookie = cookies["session-id"];
      if (sessionIdCookie == null)
      {
        yield return MonitoringStatusChanged.SessionNotPresent(target);
        continue;
      }

      _sessionId = sessionIdCookie.Value;
    }
  }

  private static bool IsSessionValid(string sessionId)
  {
    return !string.IsNullOrEmpty(sessionId) && sessionId.Length > 3;
  }

  private HttpRequestMessage CreateGenerateSessionRequest(Uri domainUrl) => new(HttpMethod.Get, domainUrl)
  {
    Headers =
    {
      { "rtt", "0" },
      { "downlink", "10" },
      { "ect", "4g" },
      { "sec-ch-ua", _siteConfig.SecUa },
      { "sec-ch-ua-mobile", "?0" },
      { "upgrade-insecure-requests", "1" },
      { "origin", _siteConfig.BaseUrl.ToString() },
      // { "content-type", "application/x-www-form-urlencoded" },
      { "user-agent", _siteConfig.Ua },
      {
        "accept",
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"
      },
      { "sec-fetch-site", "same-origin" },
      { "sec-fetch-mode", "navigate" },
      { "sec-fetch-user", "?1" },
      { "sec-fetch-dest", "document" },
      { "referer", _siteConfig.BaseUrl.ToString() },
      { "accept-encoding", "gzip, deflate, br" },
      { "accept-language", "en-US,en;q=0.9" },
    },
    Content = new FormUrlEncodedContent(Array.Empty<KeyValuePair<string?, string?>>())
  };

  public ValueTask DisposeAsync()
  {
    if (!_isDisposed)
    {
      _serviceScope.Dispose();
      _isDisposed = true;
    }

    return default;
  }

  private class CartInfo
  {
    [JsonPropertyName("isOK")] public bool IsOk { get; set; }
    [JsonPropertyName("exception")] public CartException? Exception { get; set; }
    [JsonPropertyName("items")] public IList<CartItem> Items { get; set; } = new List<CartItem>();
  }

  private class CartException
  {
    [JsonPropertyName("reason")] public string Reason { get; set; } = null!;
    [JsonPropertyName("code")] public string Code { get; set; } = null!;
  }

  private class CartItem
  {
  }
}