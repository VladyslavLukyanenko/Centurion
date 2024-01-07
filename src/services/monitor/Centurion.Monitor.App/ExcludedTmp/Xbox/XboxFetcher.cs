using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Xbox
{
  public class XboxFetcher : IProductStatusFetcher
  {
    private readonly HttpClient _httpClient;
    private readonly Uri _productUrl;

    public XboxFetcher(HttpClient httpClient, Uri productUrl)
    {
      httpClient.DefaultRequestHeaders.Add("accept",
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
      _httpClient = httpClient;
      _productUrl = productUrl;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var request = new HttpRequestMessage(HttpMethod.Get, _productUrl);
      return await StatusFetchResult.ProcessResultAsync(request, _httpClient, ct, async result =>
      {
        var ctx = BrowsingContext.New(Configuration.Default);
        var content = result.GetResponseAsString();
        var doc = await ctx.OpenAsync(_ => _.Content(content), ct);

        var checkoutBtn = doc.QuerySelector("button[aria-label='Checkout bundle']");
        var available = checkoutBtn != null && checkoutBtn.TextContent.ToUpperInvariant() != "OUT OF STOCK";

        result.AddStatus(_productUrl.ToString(), available);

        return result;
      });
    }
  }
}