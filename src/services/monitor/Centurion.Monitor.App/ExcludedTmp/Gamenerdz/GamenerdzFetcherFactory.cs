using System;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Gamenerdz
{
  [Monitor("gamenerdz")]
  public class GamenerdzFetcherFactory : ProductStatusFetcherFactoryBase
  {
    public GamenerdzFetcherFactory(IServiceProvider serviceProvider)
      : base(serviceProvider)
    {
    }
    //
    // public override Result<string> ParseRawTargetInput(string raw)
    // {
    //   if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri) || !uri.Host.Contains("gamenerdz"))
    //     return Result.Failure<string>("Invalid URL provided");
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
    //   var rawUrl = result.Value;
    //   var pageUrl = new Uri(rawUrl);
    //   var client = _monitorHttpClientFactory.CreateHttpClient();
    //   var response = await client.GetAsync(pageUrl, ct);
    //   if (!response.IsSuccessStatusCode)
    //   {
    //     throw new HttpRequestException("Non OK status code");
    //   }
    //
    //   var responseString = await response.Content.ReadAsStringAsync(ct);
    //   var ctx = BrowsingContext.New(Configuration.Default);
    //   var doc = await ctx.OpenAsync(_ => _.Content(responseString));
    //   var photo = doc.QuerySelector("div.productView > section.productView-images > figure > img").GetAttribute("src");
    //   var title = doc.GetElementsByClassName("productView-title")[0].Text();
    //   //todo: Add price when price #String
    //   client.Dispose();
    //   return new WatchTarget
    //   {
    //     Input = rawUrl,
    //     WatchersCount = 1,
    //     ShopIconUrl = "https://www.affiliatly.com/affiliate_files/brand_images/25814/Game%20Nerdz%20Logo-%20Final.png",
    //     ShopTitle = "gamenerdz.com/",
    //     Products = new Dictionary<string, ProductSummary>
    //     {
    //       {rawUrl, new ProductSummary {Sku = rawUrl, Picture = photo, Title = title, PageUrl = pageUrl}}
    //     }
    //   };
    // }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target, IMonitorHttpClientFactory clientFactory)
    {
      return new GamenerdzFetcher(clientFactory.CreateHttpClient(), new Uri(target.Input));
    }
  }
}