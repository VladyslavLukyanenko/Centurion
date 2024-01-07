using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Asus
{
  public class AsusFetcher : IProductStatusFetcher
  {
    private readonly HttpClient _httpClient;
    private readonly string _productUrl;
    private readonly IJsonSerializer _jsonSerializer;

    public AsusFetcher(HttpClient httpClient, string productUrl, IJsonSerializer jsonSerializer)
    {
      httpClient.DefaultRequestHeaders.Add("accept",
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
      httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
      httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
      _httpClient = httpClient;
      _productUrl = productUrl;
      _jsonSerializer = jsonSerializer;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var request = new HttpRequestMessage(HttpMethod.Post, "https://store.asus.com/us/category/get_real_time_data")
      {
        Content = new FormUrlEncodedContent(new[]
        {
          new KeyValuePair<string?, string?>("sm_seq_list%5B%5D%3D", _productUrl)
        })
      };
      return await StatusFetchResult.ProcessResultAsync(request, _httpClient, ct, async result =>
      {
        var data = await _jsonSerializer.DeserializeAsync<AsusData>(result.RawResponse, ct);

        var item = data!.Data[_productUrl];
        result.AddStatus(_productUrl, item.MarketInfo.Buy);

        return result;
      });
    }
  }
}