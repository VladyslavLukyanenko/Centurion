using System;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Shopify
{
  [Monitor("shopify")]
  public class ShopifyFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private readonly IJsonSerializer _jsonSerializer;

    public ShopifyFetcherFactory(IServiceProvider serviceProvider, IJsonSerializer jsonSerializer) : base(
      serviceProvider)
    {
      _jsonSerializer = jsonSerializer;
    }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target, IMonitorHttpClientFactory clientFactory)
    {
      return new ShopifyFetcher(clientFactory.CreateHttpClient(), target.Input, _jsonSerializer);
    }
  }
}