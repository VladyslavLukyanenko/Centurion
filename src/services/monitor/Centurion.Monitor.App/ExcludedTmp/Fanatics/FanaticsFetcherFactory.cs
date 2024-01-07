using System;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Fanatics
{
  [Monitor("fanatics")]
  public class FanaticsFetcherFactory : ProductStatusFetcherFactoryBase
  {
    public FanaticsFetcherFactory(IServiceProvider serviceProvider)
      : base(serviceProvider)
    {
    }
    //
    // public override Result<string> ParseRawTargetInput(string raw)
    // {
    //   if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri) || !uri.Host.Contains("fanatics"))
    //   {
    //     return Result.Failure<string>("Invalid URL provided");
    //   }
    //
    //   return raw;
    // }
    //
    // public override async ValueTask<WatchTarget> CreateTargetAsync(string raw, CancellationToken ct = default)
    // {
    //   var result = ParseRawTargetInput(raw);
    //   if (result.IsFailure)
    //   {
    //     throw new ArgumentException("Invalid raw sku value provided.", nameof(raw));
    //   }
    //
    //
    //   var rawUrl = result.Value;
    //   var url = new Uri(rawUrl);
    //   var client = _monitorHttpClientFactory.CreateHttpClient();
    //   var response = await client.GetAsync(url, ct);
    //   if (!response.IsSuccessStatusCode)
    //   {
    //     throw new HttpRequestException("Non OK status code");
    //   }
    //
    //   var responseString = await response.Content.ReadAsStringAsync(ct);
    //   var ctx = BrowsingContext.New(Configuration.Default);
    //   var doc = await ctx.OpenAsync(_ => _.Content(responseString), ct);
    //   var photo = doc.QuerySelector("img.carousel-image.current-image").GetAttribute("src");
    //   var title = doc.QuerySelector("div.layout-row.product-title > div > h1").Text();
    //   //todo: Add price when price #String
    //   client.Dispose();
    //   return new WatchTarget
    //   {
    //     Input = rawUrl,
    //     WatchersCount = 1,
    //     ShopIconUrl =
    //       "https://lever-client-logos.s3.amazonaws.com/a26ea84a-d8a3-4394-9927-2b0243d2df5a-1552380484471.png",
    //     ShopTitle = "fanatics.com",
    //     Products = new Dictionary<string, ProductSummary>
    //     {
    //       {rawUrl, new ProductSummary {Sku = rawUrl, Picture = photo, Title = title, PageUrl = url}}
    //     }
    //   };
    // }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target, IMonitorHttpClientFactory clientFactory)
    {
      return new FanaticsFetcher(clientFactory.CreateHttpClient(), new Uri(target.Input));
    }
  }
}