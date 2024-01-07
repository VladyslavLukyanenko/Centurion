using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.HomeDepot
{
  public class HomeDepotFetcher : IProductStatusFetcher
  {
    private readonly HttpClient _httpClient;
    private readonly string _itemId;
    private readonly IJsonSerializer _jsonSerializer;

    public HomeDepotFetcher(HttpClient httpClient, string itemId, IJsonSerializer jsonSerializer)
    {
      httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
      httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
      _httpClient = httpClient;
      _itemId = itemId;
      _jsonSerializer = jsonSerializer;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var requestPayload =
        $"{{\"operationName\":\"mediaPriceInventory\",\"variables\":{{\"excludeInventory\":false,\"itemIds\":[\"{_itemId}\"],\"storeId\":\"277\"}},\"query\":\"query mediaPriceInventory($excludeInventory: Boolean = false, $itemIds: [String!]!, $storeId: String!) {{\n  mediaPriceInventory(itemIds: $itemIds, storeId: $storeId) {{\n    productDetailsList {{\n      itemId\n      imageLocation\n      onlineInventory @skip(if: $excludeInventory) {{\n        enableItem\n        totalQuantity\n        __typename\n      }}\n      pricing {{\n        value\n        original\n        message\n        mapAboveOriginalPrice\n        __typename\n      }}\n      storeInventory @skip(if: $excludeInventory) {{\n        enableItem\n        totalQuantity\n        __typename\n      }}\n      __typename\n    }}\n    __typename\n  }}\n}}\n\"}}";
      var request = new HttpRequestMessage(HttpMethod.Post,
        "https://www.homedepot.com/product-information/model?opname=mediaPriceInventory")
      {
        Content = new StringContent(requestPayload)
      };
      return await StatusFetchResult.ProcessResultAsync(request, _httpClient, ct, async result =>
      {
        var data = await _jsonSerializer.DeserializeAsync<HomeDepotData>(result.RawResponse, ct);
        var available = data!.Data.MediaPriceInventory.ProductDetailsList[0].OnlineInventory.EnableItem;

        result.AddStatus(_itemId, available);

        return result;
      });
    }
  }
}