using System;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Academy
{
  [Monitor("academy")]
  public class AcademyFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private readonly IJsonSerializer _jsonSerializer;

    public AcademyFetcherFactory(IJsonSerializer jsonSerializer, IServiceProvider serviceProvider)
      : base(serviceProvider)
    {
      _jsonSerializer = jsonSerializer;
    }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target, IMonitorHttpClientFactory clientFactory)
    {
      return new AcademyFetcher(clientFactory.CreateHttpClient(), target.Input, _jsonSerializer);
    }
  }
}