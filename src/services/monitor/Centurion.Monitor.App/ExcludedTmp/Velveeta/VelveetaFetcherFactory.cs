using System;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Velveeta
{
  [Monitor("velveeta")]
  public class VelveetaFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private readonly IJsonSerializer _jsonSerializer;

    public VelveetaFetcherFactory(IServiceProvider serviceProvider, IJsonSerializer jsonSerializer)
      : base(serviceProvider)
    {
      _jsonSerializer = jsonSerializer;
    }
    //
    // public override Result<string> ParseRawTargetInput(string raw)
    // {
    //   return decimal.TryParse(raw, out _) ? raw : Result.Failure<string>("Invalid price");
    // }
    //
    // public override ValueTask<WatchTarget> CreateTargetAsync(string raw, CancellationToken ct = default)
    // {
    //   var result = ParseRawTargetInput(raw);
    //   if (result.IsFailure)
    //   {
    //     throw new ArgumentException("Invalid raw sku value provided.", nameof(raw));
    //   }
    //
    //   var target = new WatchTarget
    //   {
    //     Input = raw,
    //     ShopTitle = "velveetaliquidgold.com",
    //     ShopIconUrl =
    //       "https://www.velveetaliquidgold.com/public/COMPILED/images/lg-logo.5672e71880c97a9f500a7031d7f7769e.png",
    //     Products = new Dictionary<string, ProductSummary>
    //     {
    //       {
    //         raw,
    //         new ProductSummary
    //         {
    //           PageUrl = new Uri("https://www.velveetaliquidgold.com/"),
    //           Picture =
    //             "https://www.velveetaliquidgold.com/public/COMPILED/images/lg-logo.5672e71880c97a9f500a7031d7f7769e.png",
    //           Title = "Velveeta Movie"
    //         }
    //       }
    //     }
    //   };
    //
    //   return ValueTask.FromResult(target);
    // }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target, IMonitorHttpClientFactory clientFactory)
    {
      return new VelveetaFetcher(target, clientFactory.CreateHttpClient(), _jsonSerializer);
    }
  }
}