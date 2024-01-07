using System;
using System.Text.RegularExpressions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Amd
{
  [Monitor("amd")]
  public class AmdFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private static readonly Regex SkuRegex = new("([0-9]{10,10})", RegexOptions.Compiled);

    public AmdFetcherFactory(IServiceProvider serviceProvider)
      : base(serviceProvider)
    {
    }
    //
    // public override Result<string> ParseRawTargetInput(string raw)
    // {
    //   if (Uri.TryCreate(raw, UriKind.Absolute, out var uri))
    //   {
    //     var match = SkuRegex.Match(uri.Segments[3].TrimEnd('/'));
    //     if (match.Success && uri.Host.Contains("amd."))
    //     {
    //       return match.Groups[1].Value;
    //     }
    //   }
    //   else
    //   {
    //     var match = SkuRegex.Match(raw);
    //     if (match.Success)
    //     {
    //       return match.Groups[1].Value;
    //     }
    //   }
    //
    //   return Result.Failure<string>("Invalid format");
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
    //   var sku = result.Value;
    //   var client = _monitorHttpClientFactory.CreateHttpClient();
    //   var pageUrl = new Uri($"https://www.amd.com/en/direct-buy/{sku}/es");
    //   var response = await client.GetAsync(pageUrl, ct);
    //   if (!response.IsSuccessStatusCode)
    //   {
    //     throw new HttpRequestException("Non OK status code");
    //   }
    //
    //   var responseString = await response.Content.ReadAsStringAsync(ct);
    //   var ctx = BrowsingContext.New(Configuration.Default);
    //   var doc = await ctx.OpenAsync(_ => _.Content(responseString), ct);
    //   var photo = doc
    //     .QuerySelector(
    //       "#product-details-info > div.container > div > div.product-page-image.col-flex-lg-7.col-flex-sm-12")
    //     .Children[0].GetAttribute("src");
    //   var title = doc
    //     .QuerySelector(
    //       "#product-details-info > div.container > div > div.product-page-description.col-flex-lg-5.col-flex-sm-12")
    //     .Children[0].Text();
    //   //todo: Add price when price #String
    //   client.Dispose();
    //   return new WatchTarget
    //   {
    //     Input = sku,
    //     WatchersCount = 1,
    //     ShopIconUrl = "https://logos-world.net/wp-content/uploads/2020/03/AMD-Logo.png",
    //     ShopTitle = "amd.com",
    //     Products = new Dictionary<string, ProductSummary>
    //     {
    //       {sku, new ProductSummary {Sku = sku, Picture = photo, Title = title, PageUrl = pageUrl}}
    //     }
    //   };
    // }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target, IMonitorHttpClientFactory clientFactory)
    {
      return new AmdFetcher(clientFactory.CreateHttpClient(), target.Input);
    }
  }
}