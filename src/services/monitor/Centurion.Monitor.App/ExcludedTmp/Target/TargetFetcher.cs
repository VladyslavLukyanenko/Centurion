using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Centurion.Monitor.App.Sites.Target.QuickType;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Target
{
  public class TargetFetcher : IProductStatusFetcher
  {
    private readonly HttpClient _httpClient;
    private readonly string _sku;
    private readonly IJsonSerializer _jsonSerializer;

    public TargetFetcher(HttpClient httpClient, string sku, IJsonSerializer jsonSerializer)
    {
      httpClient.DefaultRequestHeaders.Add("accept",
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
      _httpClient = httpClient;
      _sku = sku;
      _jsonSerializer = jsonSerializer;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var requestUri = "https://redsky.target.com/redsky_aggregations/v1/apps/pdp_v2?tcin=" + _sku
        + "&store_id=1234&pricing_store_id=1234&scheduled_delivery_store_id=1234&device_type=android&key=5d546952f5059b16db4aa90913e56d09d3ff2aa4";
      var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
      return await StatusFetchResult.ProcessResultAsync(request, _httpClient, ct, async result =>
      {
        var responseStruct = await _jsonSerializer.DeserializeAsync<TargetData>(result.RawResponse, ct);
        var available = responseStruct!.Data.Product.Fulfillment.ShippingOptions.AvailabilityStatus == "IN_STOCK";
        result.AddStatus(_sku, available);

        return result;
      });
    }
  }
}