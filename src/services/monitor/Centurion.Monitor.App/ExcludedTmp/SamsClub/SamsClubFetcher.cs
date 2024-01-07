using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.SamsClub
{
  // todo add multi-product support
  public class SamsClubFetcher : IProductStatusFetcher
  {
    private readonly HttpClient _httpClient;
    private readonly string _productId;
    private readonly IJsonSerializer _jsonSerializer;

    public SamsClubFetcher(HttpClient httpClient, string productId, IJsonSerializer jsonSerializer)
    {
      httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
      httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
      httpClient.DefaultRequestHeaders.Add("authority", "www.samsclub.com");
      httpClient.DefaultRequestHeaders.Add("accept", "application/json, text/plain, */*");
      httpClient.DefaultRequestHeaders.Add("origin", "https://www.samsclub.com");
      httpClient.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
      httpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
      httpClient.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
      httpClient.DefaultRequestHeaders.Add("referer",
        "https://www.samsclub.com/p/mmboatisland-boatisland2021/prod25241579?xid=plp_product_5");
      httpClient.DefaultRequestHeaders.Add("accept-language", "es-ES,es;q=0.9,ru;q=0.8");
      _httpClient = httpClient;
      _productId = productId;
      _jsonSerializer = jsonSerializer;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var requestUri = "https://www.samsclub.com/api/node/vivaldi/v2/az/products";
      var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
      {
        Content = new StringContent($"{{\"productIds\":[\"{_productId}\"],\"type\":\"LARGE\",\"clubId\":\"\"}}",
          Encoding.UTF8, "application/json")
      };

      return await StatusFetchResult.ProcessResultAsync(request, _httpClient, ct, async result =>
      {
        var data = await _jsonSerializer.DeserializeAsync<SamsClubData>(result.RawResponse, ct);
        var available = data!.Status == "SUCCESS" &&
                        data.Payload.Products.Length > 0 &&
                        data.Payload.Products[0].Skus.Length > 0 &&
                        data.Payload.Products[0].Skus[0].OnlineOffer.OfferStatus == "PURCHASABLE";

        result.AddStatus(_productId, available);

        return result;
      });
    }
  }
}