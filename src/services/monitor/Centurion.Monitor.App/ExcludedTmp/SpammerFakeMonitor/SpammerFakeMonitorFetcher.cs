using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.SpammerFakeMonitor
{
  public class SpammerFakeMonitorFetcher : IProductStatusFetcher
  {
    private bool _isAvailable;
    public ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var r = StatusFetchResult.NewEmpty();
      var isAvailable = _isAvailable = !_isAvailable;
      r.AddStatus("spammer", isAvailable);
      return new ValueTask<Result<StatusFetchResult>>(r);
    }
  }
}