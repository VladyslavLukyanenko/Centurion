using System;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.HomeDepot
{
  [Monitor("homedepot")]
  public class HomeDepotFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private readonly IJsonSerializer _jsonSerializer;

    public HomeDepotFetcherFactory(IServiceProvider serviceProvider, IJsonSerializer jsonSerializer)
      : base(serviceProvider)
    {
      _jsonSerializer = jsonSerializer;
    }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target, IMonitorHttpClientFactory clientFactory)
    {
      return new HomeDepotFetcher(clientFactory.CreateHttpClient(), target.Input, _jsonSerializer);
    }
  }
}