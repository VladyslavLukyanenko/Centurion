using System;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Tesla
{
  [Monitor("tesla")]
  public class TeslaFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private readonly IJsonSerializer _jsonSerializer;

    public TeslaFetcherFactory(IServiceProvider serviceProvider, IJsonSerializer jsonSerializer) : base(serviceProvider)
    {
      _jsonSerializer = jsonSerializer;
    }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target, IMonitorHttpClientFactory clientFactory)
    {
      return new TeslaFetcher(clientFactory.CreateHttpClient(), target.Input, _jsonSerializer);
    }
  }
}