using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Bestbuy
{
  public class BestbuyFetcher : IProductStatusFetcher
  {
    private const string ExpectedInStockStatus = "ADD_TO_CART";
    private readonly string _skus;
    private readonly HttpClient _httpClient;
    private readonly IJsonSerializer _jsonSerializer;

    public BestbuyFetcher(string skus, HttpClient httpClient, IJsonSerializer jsonSerializer)
    {
      _skus = skus;

      httpClient.DefaultRequestHeaders.Add("User-Agent",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.8.431.141 Safari/537.36");
      httpClient.DefaultRequestHeaders.Add("accept", "application/json");
      _httpClient = httpClient;
      _jsonSerializer = jsonSerializer;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var requestUri = "https://www.bestbuy.com/api/3.0/priceBlocks?skus=" + _skus;
      var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
      return await StatusFetchResult.ProcessResultAsync(message, _httpClient, ct, async result =>
      {
        var data = await _jsonSerializer.DeserializeAsync<IList<BestbuyData>>(result.RawResponse, ct);
        foreach (var prod in data!)
        {
          var available = prod.Sku.ButtonState.ButtonState == ExpectedInStockStatus;
          result.AddStatus(prod.Sku.SkuId, available);
        }

        return result;
      });
    }
  }
}