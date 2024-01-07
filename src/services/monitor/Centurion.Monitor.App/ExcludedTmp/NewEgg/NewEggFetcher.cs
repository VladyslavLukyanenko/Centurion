using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.NewEgg
{
  public class NewEggFetcher : IProductStatusFetcher
  {
    private readonly HttpClient _httpClient;
    private readonly string _itemNum;
    private readonly IJsonSerializer _jsonSerializer;

    public NewEggFetcher(HttpClient httpClient, string itemNum, IJsonSerializer jsonSerializer)
    {
      httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
      httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
      _httpClient = httpClient;
      _itemNum = itemNum;
      _jsonSerializer = jsonSerializer;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var requestUri = "https://www.newegg.com/product/api/ProductRealtime?ItemNumber=" + _itemNum;
      var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
      return await StatusFetchResult.ProcessResultAsync(request, _httpClient, ct, async result =>
      {
        var data = await _jsonSerializer.DeserializeAsync<NewEggData>(result.RawResponse, ct);
        if (data == null)
        {
          result.AddStatus(_itemNum, false);
          return result;
        }

        var available = data.MainItem.Instock;

        result.AddStatus(_itemNum, available);

        return result;
      });
    }
  }
}