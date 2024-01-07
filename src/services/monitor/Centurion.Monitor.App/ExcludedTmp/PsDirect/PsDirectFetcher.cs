using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.PsDirect
{
  public class PsDirectFetcher : IProductStatusFetcher
  {
    private readonly HttpClient _httpClient;
    private readonly string _productCode;
    private readonly IJsonSerializer _jsonSerializer;

    public PsDirectFetcher(HttpClient httpClient, string productCode, IJsonSerializer jsonSerializer)
    {
      httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
      httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
      _httpClient = httpClient;
      _productCode = productCode;
      _jsonSerializer = jsonSerializer;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var requestUri =
        "https://api.direct.playstation.com/commercewebservices/ps-direct-us/users/anonymous/products/productList?fields=BASIC&productCodes="
        + _productCode;
      var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
      return await StatusFetchResult.ProcessResultAsync(request, _httpClient, ct, async result =>
      {
        var data = await _jsonSerializer.DeserializeAsync<PlayStationDirectData>(result.RawResponse, ct);
        var prod = data!.Products[0];
        var available = prod.Stock.StockLevelStatus != "outOfStock";

        result.AddStatus(_productCode, available);

        return result;
      });
    }
  }
}