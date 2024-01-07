using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Shopify
{
  public class ShopifyFetcher : IProductStatusFetcher
  {
    private readonly HttpClient _httpClient;
    private readonly string _handle;
    private readonly IJsonSerializer _jsonSerializer;

    public ShopifyFetcher(HttpClient httpClient, string handle, IJsonSerializer jsonSerializer)
    {
      httpClient.DefaultRequestHeaders.Add("accept",
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
      _httpClient = httpClient;
      _handle = handle;
      _jsonSerializer = jsonSerializer;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var request = new HttpRequestMessage(HttpMethod.Get, "https://collectiveminds-uk.myshopify.com/products.json")
      {
        Content = new StringContent($"[\"{_handle}\"]", Encoding.UTF8, "application/json")
      };
      // todo: add multi-handle support
      return await StatusFetchResult.ProcessResultAsync(request, _httpClient, ct, async result =>
      {
        var data = await _jsonSerializer.DeserializeAsync<ShopifyData>(result.RawResponse, ct);
        var product = data?.Products.FirstOrDefault(_ => _.Handle == _handle);
        if (product == null)
        {
          return Result.Failure<StatusFetchResult>("Product not found. Invalid handle " + _handle);
        }

        var available = product.Variants.Any(_ => _.Available);
        result.AddStatus(_handle, available);

        return result;
      });
    }
  }
}