using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.SpammerFakeMonitor
{
  [Monitor("spammer_fake_monitor")]
  public class SpammerFakeMonitorFetcherFactory : ProductStatusFetcherFactoryBase
  {
    public SpammerFakeMonitorFetcherFactory(IServiceProvider serviceProvider)
      : base(serviceProvider)
    {
    }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target, IMonitorHttpClientFactory clientFactory) => new SpammerFakeMonitorFetcher();
  }
}