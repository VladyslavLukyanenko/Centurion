using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.DicksSportingGoods
{
  public class DicksSportingGoodsFactory : IProductStatusFetcher
  {
    private readonly HttpClient _httpClient;
    private readonly string _productId;
    private readonly IJsonSerializer _jsonSerializer;

    public DicksSportingGoodsFactory(HttpClient httpClient, string productId, IJsonSerializer jsonSerializer)
    {
      httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
      httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
      _httpClient = httpClient;
      _productId = productId;
      _jsonSerializer = jsonSerializer;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var request = new HttpRequestMessage(HttpMethod.Get,
        "https://availability.dickssportinggoods.com/v1/inventoryapis/searchinventory?location=0&sku=" + _productId);
      return await StatusFetchResult.ProcessResultAsync(request, _httpClient, ct, async result =>
      {
        var data = await _jsonSerializer.DeserializeAsync<DicksSportingGoodsData>(result.RawResponse, ct);
        var available = int.Parse(data!.Data.Skus[0].Atsqty) > 0;

        result.AddStatus(_productId, available);

        return result;
      });
    }
  }
}