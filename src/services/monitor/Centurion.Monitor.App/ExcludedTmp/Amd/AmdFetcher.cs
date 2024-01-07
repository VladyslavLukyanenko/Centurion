using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Amd
{
  public class AmdFetcher : IProductStatusFetcher
  {
    private readonly HttpClient _httpClient;
    private readonly string _sku;

    public AmdFetcher(HttpClient httpClient, string sku)
    {
      httpClient.DefaultRequestHeaders.Add("accept",
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
      httpClient.DefaultRequestHeaders.Add("referer", "https://www.amd.com");
      _httpClient = httpClient;
      _sku = sku;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var requestUri = $"https://www.amd.com/en/direct-buy/{_sku}/en?cac=134131";
      var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
      return await StatusFetchResult.ProcessResultAsync(request, _httpClient, ct, async result =>
      {
        var ctx = BrowsingContext.New(Configuration.Default);
        var doc = await ctx.OpenAsync(_ => _.Content(result.RawResponse), ct);
        var captcha = doc.QuerySelector("#captchacharacters");
        if (captcha != null)
        {
          return Result.Failure<StatusFetchResult>("protected by captcha");
        }

        var availabilityBlock = doc.QuerySelector(".product-out-of-stock");
        var available = availabilityBlock == null;
        result.AddStatus(_sku, available);

        return result;
      });
    }
  }
}