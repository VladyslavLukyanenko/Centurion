using System;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.BarnesAndNoble
{
  [Monitor("barnesandnoble")]
  public class BarnesAndNobleFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private readonly IJsonSerializer _jsonSerializer;

    public BarnesAndNobleFetcherFactory(IJsonSerializer jsonSerializer, IServiceProvider serviceProvider)
      : base(serviceProvider)
    {
      _jsonSerializer = jsonSerializer;
    }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target, IMonitorHttpClientFactory clientFactory)
    {
      return new BarnesAndNobleFetcher(clientFactory.CreateHttpClient(), new Uri(target.Input), _jsonSerializer);
    }
  }
}