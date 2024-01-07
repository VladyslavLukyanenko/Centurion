using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Staples
{
  public class StaplesFetcher : IProductStatusFetcher
  {
    private readonly HttpClient _httpClient;
    private readonly string _sku;
    private readonly IJsonSerializer _jsonSerializer;

    public StaplesFetcher(HttpClient httpClient, string sku, IJsonSerializer jsonSerializer)
    {
      httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
      httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
      _httpClient = httpClient;
      _sku = sku;
      _jsonSerializer = jsonSerializer;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var requestUri = "https://www.staples.com/ele-lpd/api/sku/" + _sku;
      var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
      return await StatusFetchResult.ProcessResultAsync(request, _httpClient, ct, async result =>
      {
        var data = await _jsonSerializer.DeserializeAsync<StaplesData>(result.RawResponse, ct);

        var available = !data!.SkuState.SkuData.Items[0].Inventory.Items[0].ProductIsOutOfStock;
        result.AddStatus(_sku, available);

        return result;
      });
    }
  }
}