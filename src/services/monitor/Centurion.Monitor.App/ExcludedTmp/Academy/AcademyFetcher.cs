using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Academy
{
  public class AcademyFetcher : IProductStatusFetcher
  {
    private readonly HttpClient _httpClient;
    private readonly string _productId;
    private readonly IJsonSerializer _jsonSerializer;

    public AcademyFetcher(HttpClient httpClient, string productId, IJsonSerializer jsonSerializer)
    {
      httpClient.DefaultRequestHeaders.Add("accept",
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
      _httpClient = httpClient;
      _productId = productId;
      _jsonSerializer = jsonSerializer;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var requestUri =
        $"https://www.academy.com/api/inventory?productId={_productId}&storeId=&storeEligibility=&bopisEnabled=true&isSTSEnabled=false";
      var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
      return await StatusFetchResult.ProcessResultAsync(request, _httpClient, ct, async result =>
      {
        var data = await _jsonSerializer.DeserializeAsync<AcademyData>(result.RawResponse, ct);
        if (data!.Online.Count == 0)
        {
          result.AddStatus(_productId, false);
        }
        else
        {
          foreach (var item in data.Online)
          {
            var available = item.InventoryStatus == "available";

            result.AddStatus(item.SkuId, available);
          }
        }

        return result;
      });
    }
  }
}