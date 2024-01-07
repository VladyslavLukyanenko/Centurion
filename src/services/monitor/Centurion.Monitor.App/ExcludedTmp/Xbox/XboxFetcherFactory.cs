using System;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Xbox
{
  [Monitor("xbox")]
  public class XboxFetcherFactory : ProductStatusFetcherFactoryBase
  {
    public XboxFetcherFactory(IServiceProvider serviceProvider)
      : base(serviceProvider)
    {
    }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target, IMonitorHttpClientFactory clientFactory)
    {
      return new XboxFetcher(clientFactory.CreateHttpClient(), new Uri(target.Input));
    }
  }
}