using System;
using System.Text.RegularExpressions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.PsDirect
{
  [Monitor("psdirect")]
  public class PsDirectFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private static readonly Regex SkuRegex = new("([0-9]{7,7})", RegexOptions.Compiled);
    private readonly IJsonSerializer _jsonSerializer;

    public PsDirectFetcherFactory(IServiceProvider serviceProvider, IJsonSerializer jsonSerializer)
      : base(serviceProvider)
    {
      _jsonSerializer = jsonSerializer;
    }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target, IMonitorHttpClientFactory clientFactory)
    {
      return new PsDirectFetcher(clientFactory.CreateHttpClient(), target.Input, _jsonSerializer);
    }
  }
}