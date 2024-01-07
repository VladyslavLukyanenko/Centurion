using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Staples
{
  [Monitor("staples")]
  public class StaplesFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private readonly IJsonSerializer _jsonSerializer;

    public StaplesFetcherFactory(IServiceProvider serviceProvider, IJsonSerializer jsonSerializer)
      : base(serviceProvider)
    {
      _jsonSerializer = jsonSerializer;
    }
    //
    // public override Result<string> ParseRawTargetInput(string raw)
    // {
    //   if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri) || !uri.Host.Contains("staples"))
    //   {
    //     return Result.Failure<string>("Invalid URL provided");
    //   }
    //
    //   return $"{uri.Segments[1]}{uri.Segments[2]}";
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
    //   var sku = result.Value;
    //   var client = _monitorHttpClientFactory.CreateHttpClient();
    //   var pageUrl = new Uri($"https://www.staples.com/{sku}");
    //   var response = await client.GetAsync(pageUrl, ct);
    //   if (!response.IsSuccessStatusCode)
    //   {
    //     throw new HttpRequestException("Non OK status code");
    //   }
    //
    //   var responseString = await response.Content.ReadAsStringAsync(ct);
    //   var ctx = BrowsingContext.New(Configuration.Default);
    //   var doc = await ctx.OpenAsync(_ => _.Content(responseString), ct);
    //   var photo = doc.QuerySelector("#thumb0 > img").GetAttribute("src");
    //   var title = doc.GetElementById("product_title").Text();
    //   //todo: Add price when price #String
    //   client.Dispose();
    //   return new WatchTarget
    //   {
    //     Input = sku,
    //     WatchersCount = 1,
    //     ShopIconUrl = "https://cdn.mos.cms.futurecdn.net/9FjXgFet9VcH4fXyqvva2j.jpg",
    //     ShopTitle = "staples.com",
    //     Products = new Dictionary<string, ProductSummary>
    //     {
    //       {
    //         sku,
    //         new ProductSummary {Sku = sku, Picture = photo, Title = title, PageUrl = pageUrl}
    //       }
    //     }
    //   };
    // }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target, IMonitorHttpClientFactory clientFactory)
    {
      return new StaplesFetcher(clientFactory.CreateHttpClient(), target.Input, _jsonSerializer);
    }
  }
}