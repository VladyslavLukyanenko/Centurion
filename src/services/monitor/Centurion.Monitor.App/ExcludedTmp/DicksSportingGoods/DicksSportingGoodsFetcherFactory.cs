using System;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.DicksSportingGoods
{
  [Monitor("dickssportinggoods")]
  public class DicksSportingGoodsFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private readonly IJsonSerializer _jsonSerializer;

    public DicksSportingGoodsFetcherFactory(IServiceProvider serviceProvider, IJsonSerializer jsonSerializer)
      : base(serviceProvider)
    {
      _jsonSerializer = jsonSerializer;
    }
    //
    // public override Result<string> ParseRawTargetInput(string raw)
    // {
    //   throw new NotImplementedException();
    // }
    //
    // public override ValueTask<WatchTarget> CreateTargetAsync(string raw, CancellationToken ct = default)
    // {
    //   var result = ParseRawTargetInput(raw);
    //   if (result.IsFailure)
    //   {
    //     throw new ArgumentException("Invalid raw sku value provided.", nameof(raw));
    //   }
    //   throw new NotImplementedException();
    // }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target, IMonitorHttpClientFactory clientFactory)
    {
      return new DicksSportingGoodsFactory(clientFactory.CreateHttpClient(), target.Input, _jsonSerializer);
    }
  }
}