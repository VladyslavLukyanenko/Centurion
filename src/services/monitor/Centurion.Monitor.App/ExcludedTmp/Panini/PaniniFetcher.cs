using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Panini
{
  public class PaniniFetcher : IProductStatusFetcher
  {
    private readonly HttpClient _httpClient;
    private readonly string _itemId;
    private readonly IJsonSerializer _jsonSerializer;

    public PaniniFetcher(HttpClient httpClient, string itemId, IJsonSerializer jsonSerializer)
    {
      httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
      httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
      _httpClient = httpClient;
      _itemId = itemId;
      _jsonSerializer = jsonSerializer;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var requestPayload = "{\"operationName\":\"ItemDetailsQuery\",\"variables\":{\"sku\":\"\",\"name\":$\"" + _itemId
        + "\"}}},\"query\":\"query ItemDetailsQuery($sku: String!, $name : String) { item(attribute_code: [\\\"description\\\", \\\"short_description\\\", \\\"category_ids\\\", \\\"reward_points\\\", \\\"news_from_date\\\", \\\"news_to_date\\\", \\\"special_price\\\", \\\"news_from_date\\\", \\\"news_to_date\\\", \\\"special_from_date\\\", \\\"special_to_date\\\", \\\"pan_offer_start_date\\\", \\\"pan_offer_end_date\\\"], sku: $sku, name: $name) { __typename id sku name is_in_stock } }\"}";
      var request = new HttpRequestMessage(HttpMethod.Post,
        "https://www.homedepot.com/product-information/model?opname=mediaPriceInventory")
      {
        Content = new StringContent(requestPayload)
      };
      return await StatusFetchResult.ProcessResultAsync(request, _httpClient, ct, async result =>
      {
        var data = await _jsonSerializer.DeserializeAsync<PaniniData>(result.RawResponse, ct);
        var available = data!.Data.Item.IsInStock;

        result.AddStatus(_itemId, available);

        return result;
      });
    }
  }
}