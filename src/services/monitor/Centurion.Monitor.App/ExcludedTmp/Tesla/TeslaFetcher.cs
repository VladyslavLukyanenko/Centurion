using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Tesla
{
  public class TeslaFetcher : IProductStatusFetcher
  {
    private readonly HttpClient _httpClient;
    private readonly string _sku;
    private readonly IJsonSerializer _jsonSerializer;

    public TeslaFetcher(HttpClient httpClient, string sku, IJsonSerializer jsonSerializer)
    {
      httpClient.DefaultRequestHeaders.Add("accept",
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
      httpClient.DefaultRequestHeaders.Add("authority", "shop.tesla.com");
      httpClient.DefaultRequestHeaders.Add("pragma", "no-cache");
      httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
      httpClient.DefaultRequestHeaders.Add("accept", "application/json, text/javascript, */*; q=0.01");
      httpClient.DefaultRequestHeaders.Add("x-requested-with", "XMLHttpRequest");
      httpClient.DefaultRequestHeaders.Add("content-type", "application/json");
      httpClient.DefaultRequestHeaders.Add("origin", "https://shop.tesla.com");
      httpClient.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
      httpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
      httpClient.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
      httpClient.DefaultRequestHeaders.Add("referer", "https://shop.tesla.com/product/tesla-tequila");
      httpClient.DefaultRequestHeaders.Add("accept-language", "es-ES,es;q=0.9,ru;q=0.8");
      _httpClient = httpClient;
      _sku = sku;
      _jsonSerializer = jsonSerializer;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var request = new HttpRequestMessage(HttpMethod.Post, "https://shop.tesla.com/inventory.json")
      {
        Content = new StringContent($"[\"{_sku}\"]", Encoding.UTF8, "application/json")
      };
      return await StatusFetchResult.ProcessResultAsync(request, _httpClient, ct, async result =>
      {
        var data = await _jsonSerializer.DeserializeAsync<List<TeslaData>>(result.RawResponse, ct);
        var available = data?.FirstOrDefault()?.Purchasable ?? false;

        result.AddStatus(_sku, available);

        return result;
      });
    }
  }
}