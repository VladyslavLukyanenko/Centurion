using System;
using System.Text.RegularExpressions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.SamsClub
{
  [Monitor("samsclub")]
  public class SamsClubFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private static readonly Regex SkuRegex = new("([0-9a-z]{12,12})", RegexOptions.Compiled);
    private readonly IJsonSerializer _jsonSerializer;

    public SamsClubFetcherFactory(IServiceProvider serviceProvider, IJsonSerializer jsonSerializer)
      : base(serviceProvider)
    {
      _jsonSerializer = jsonSerializer;
    }
    //
    // public override Result<string> ParseRawTargetInput(string raw)
    // {
    //   if (Uri.TryCreate(raw, UriKind.Absolute, out var uri))
    //   {
    //     var match = SkuRegex.Match(uri.Segments[3]);
    //     if (match.Success && uri.Host.Contains("samsclub"))
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
    //   return Result.Failure<string>("Invalid format provided");
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
    //   var pageUrl = new Uri($"https://www.samsclub.com/p/-/{sku}");
    //   var response = await client.GetAsync(pageUrl, ct);
    //   if (!response.IsSuccessStatusCode)
    //   {
    //     throw new HttpRequestException("Non OK status code");
    //   }
    //
    //   var responseString = await response.Content.ReadAsStringAsync(ct);
    //   var ctx = BrowsingContext.New(Configuration.Default);
    //   var doc = await ctx.OpenAsync(_ => _.Content(responseString), ct);
    //   //todo: Add photo parsing
    //   var
    //     photo = ""; //doc.QuerySelector("div.row.d-flex > div.ce-p-images > div.p-image > div.top_level").Children[0].GetAttribute("src");
    //   var title = doc
    //     .QuerySelector(
    //       "#main > div > div > div.sc-pc-large-desktop-product-card > div > div.sc-pc-title-full-desktop > h1").Text();
    //   //todo: Add price when price #String
    //   client.Dispose();
    //   return new WatchTarget
    //   {
    //     Input = sku,
    //     WatchersCount = 1,
    //     ShopIconUrl =
    //       "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e7/Sams_Club.svg/1024px-Sams_Club.svg.png",
    //     ShopTitle = "samsclub.com",
    //     Products = new Dictionary<string, ProductSummary>
    //     {
    //       {sku, new ProductSummary {Sku = sku, Picture = photo, Title = title, PageUrl = pageUrl}}
    //     }
    //   };
    // }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target, IMonitorHttpClientFactory clientFactory)
    {
      return new SamsClubFetcher(clientFactory.CreateHttpClient(), target.Input, _jsonSerializer);
    }
  }
}