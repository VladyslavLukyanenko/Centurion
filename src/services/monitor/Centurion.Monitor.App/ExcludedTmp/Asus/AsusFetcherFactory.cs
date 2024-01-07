using System;
using System.Text.RegularExpressions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Asus
{
  [Monitor("asus")]
  public class AsusFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private static readonly Regex SkuRegex = new Regex("([0-9A-Z]{17,17})", RegexOptions.Compiled);
    private readonly IJsonSerializer _jsonSerializer;

    public AsusFetcherFactory(IServiceProvider serviceProvider, IJsonSerializer jsonSerializer) : base(serviceProvider)
    {
      _jsonSerializer = jsonSerializer;
    }

    // public override Result<string> ParseRawTargetInput(string raw)
    // {
    //   if (Uri.TryCreate(raw, UriKind.Absolute, out var uri))
    //   {
    //     var match = SkuRegex.Match(uri.Segments[3]);
    //     if (match.Success && uri.Host.Contains("store.asus"))
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
    //   var pageUrl = new Uri($"https://store.asus.com/us/item/{sku}");
    //   var response = await client.GetAsync(pageUrl, ct);
    //   if (!response.IsSuccessStatusCode)
    //   {
    //     throw new HttpRequestException("Non OK status code");
    //   }
    //
    //   var responseString = await response.Content.ReadAsStringAsync(ct);
    //   var ctx = BrowsingContext.New(Configuration.Default);
    //   var doc = await ctx.OpenAsync(_ => _.Content(responseString), ct);
    //   var photo = doc.GetElementById("item_photo").GetAttribute("src");
    //   var title = doc.GetElementById("pro_title").Text();
    //   //todo: Add price when price #String
    //   client.Dispose();
    //   return new WatchTarget
    //   {
    //     Input = sku,
    //     WatchersCount = 1,
    //     ShopIconUrl = "https://logos-marcas.com/wp-content/uploads/2020/07/Asus-Logo.png",
    //     ShopTitle = "asus.com",
    //     Products = new Dictionary<string, ProductSummary>
    //     {
    //       {sku, new ProductSummary {Sku = sku, Picture = photo, Title = title, PageUrl = pageUrl}}
    //     }
    //   };
    // }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target, IMonitorHttpClientFactory clientFactory)
    {
      return new AsusFetcher(clientFactory.CreateHttpClient(), target.Input, _jsonSerializer);
    }
  }
}