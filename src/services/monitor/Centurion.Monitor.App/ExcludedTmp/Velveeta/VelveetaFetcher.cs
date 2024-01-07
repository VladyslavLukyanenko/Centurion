using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Velveeta
{
  public class VelveetaFetcher : IProductStatusFetcher
  {
    private readonly decimal _expectedPrice;
    private readonly WatchTarget _target;
    private readonly HttpClient _httpClient;
    private readonly IJsonSerializer _jsonSerializer;

    public VelveetaFetcher(WatchTarget target, HttpClient httpClient, IJsonSerializer jsonSerializer)
    {
      _expectedPrice = decimal.Parse(target.Input);

      httpClient.DefaultRequestHeaders.Add("User-Agent",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.8.431.141 Safari/537.36");
      httpClient.DefaultRequestHeaders.Add("accept", "application/json");
      _target = target;
      _httpClient = httpClient;
      _jsonSerializer = jsonSerializer;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      const string requestUri = "https://www.velveetaliquidgold.com/v1/kraft/velveetagoldrush/market/current.json";
      var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
      return await StatusFetchResult.ProcessResultAsync(message, _httpClient, ct, async result =>
      {
        var data = await _jsonSerializer.DeserializeAsync<VelveetaData>(result.RawResponse, ct);
        var available = data!.Result.Value >= _expectedPrice;
        var delayRequest = TimeSpan.FromSeconds(data.Result.ExpiresSeconds);

        var product = _target.Product;
        // product.Price = data.Result.Value.ToString(CultureInfo.InvariantCulture);
        result.AddStatus(_target.Input, available, delayRequest);

        return result;
      });
    }
  }
}