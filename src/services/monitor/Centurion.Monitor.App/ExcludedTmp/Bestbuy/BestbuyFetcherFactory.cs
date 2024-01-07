using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Bestbuy
{
  [Monitor("bestbuy")]
  public class BestbuyFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private const char SkusDelim = ',';

    private static readonly Regex SkuRegex = new("([0-9]{7,7})", RegexOptions.Compiled);
    private readonly IJsonSerializer _jsonSerializer;

    public BestbuyFetcherFactory(IServiceProvider serviceProvider, IJsonSerializer jsonSerializer)
      : base(serviceProvider)
    {
      _jsonSerializer = jsonSerializer;
    }
    //
    // public override Result<string> ParseRawTargetInput(string raw)
    // {
    //   if (Uri.TryCreate(raw, UriKind.Absolute, out var uri))
    //   {
    //     var match = SkuRegex.Match(uri.Segments[3].TrimEnd('p').TrimEnd('.'));
    //     if (match.Success && uri.Host.Contains("bestbuy"))
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
    //   var pageUrl = new Uri($"https://www.bestbuy.com/site/-/{sku}.p?intl=nosplash");
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
    //       "#shop-media-gallery-15490922 > div > div > div > div.primary-media-wrapper.base-page-image.lv > div")
    //     .Children[0].Children[0].GetAttribute("src");
    //   var title = doc.GetElementsByClassName("sku-title")[0].Children[0].Text();
    //   //todo: Add price when price #String
    //   client.Dispose();
    //   return new WatchTarget
    //   {
    //     Input = sku,
    //     WatchersCount = 1,
    //     ShopIconUrl = "https://i1.wp.com/elpoderdelasideas.com/wp-content/uploads/best-buy-3.png?w=329&h=325&ssl=1",
    //     ShopTitle = "bestbuy.com",
    //     Products = new Dictionary<string, ProductSummary>
    //     {
    //       {
    //         sku,
    //         new ProductSummary
    //         {
    //           Sku = sku, Picture = photo, Title = title,
    //           PageUrl = pageUrl
    //         }
    //       }
    //     }
    //   };
    // }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target, IMonitorHttpClientFactory clientFactory)
    {
      var distinctSkus = string.Join(SkusDelim, GetTargets(target.Input));
      return new BestbuyFetcher(distinctSkus, clientFactory.CreateHttpClient(), _jsonSerializer);
    }

    private IList<string> GetTargets(string productUrl) =>
      productUrl.Split(SkusDelim, StringSplitOptions.RemoveEmptyEntries)
        .Distinct()
        .ToList();
  }
}