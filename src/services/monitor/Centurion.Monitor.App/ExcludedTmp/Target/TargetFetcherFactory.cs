using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Target
{
  [Monitor("target")]
  public class TargetFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private static readonly Regex SkuRegex = new("A-([0-9]{8,8})", RegexOptions.Compiled);
    private readonly IJsonSerializer _jsonSerializer;

    public TargetFetcherFactory(IJsonSerializer jsonSerializer, IServiceProvider serviceProvider)
      : base(serviceProvider)
    {
      _jsonSerializer = jsonSerializer;
    }/*

    public override Result<string> ParseRawTargetInput(string raw)
    {
      if (Uri.TryCreate(raw, UriKind.Absolute, out var uri))
      {
        var match = SkuRegex.Match(raw);
        if (match.Success && uri.Host.Contains("target"))
        {
          return match.Groups[1].Value;
        }
      }
      else
      {
        var match = SkuRegex.Match(raw);
        if (match.Success)
        {
          return match.Groups[1].Value;
        }
      }

      return Result.Failure<string>("Invalid format provided");
    }

    public override async ValueTask<WatchTarget> CreateTargetAsync(string raw, CancellationToken ct = default)
    {
      var result = ParseRawTargetInput(raw);
      if (result.IsFailure)
      {
        throw new ArgumentException("Invalid raw sku value provided.", nameof(raw));
      }

      var sku = result.Value;
      var client = _monitorHttpClientFactory.CreateHttpClient();
      var pageUrl = new Uri($"https://www.target.com/p/-/A-{sku}");
      var response = await client.GetAsync(pageUrl, ct);
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException("Non OK status code");
      }

      var responseString = await response.Content.ReadAsStringAsync(ct);
      var ctx = BrowsingContext.New(Configuration.Default);
      var doc = await ctx.OpenAsync(_ => _.Content(responseString), ct);
      var photo = doc.QuerySelector("div > div:nth-child(1) > button > img").GetAttribute("src");
      var title = doc.QuerySelector("div:nth-child(2) > h1 > span").Text();
      //todo: Add price when price #String
      client.Dispose();
      return new WatchTarget
      {
        Input = sku,
        WatchersCount = 1,
        ShopIconUrl =
          "https://upload.wikimedia.org/wikipedia/commons/thumb/9/9a/Target_logo.svg/771px-Target_logo.svg.png",
        ShopTitle = "target.com",
        Products = new Dictionary<string, ProductSummary>
        {
          {sku, new ProductSummary {Sku = sku, Picture = photo, Title = title, PageUrl = pageUrl}}
        }
      };
    }*/

    public override IProductStatusFetcher CreateFetcher(WatchTarget target, IMonitorHttpClientFactory clientFactory)
    {
      return new TargetFetcher(clientFactory.CreateHttpClient(), target.Input, _jsonSerializer);
    }
  }
}