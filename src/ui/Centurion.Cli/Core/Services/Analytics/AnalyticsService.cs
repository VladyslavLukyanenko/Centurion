using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Centurion.Cli.Core.Services.Modules;
using Centurion.Contracts.Analytics;
using CSharpFunctionalExtensions;
using DynamicData;
using Google.Protobuf.WellKnownTypes;
using NodaTime;

namespace Centurion.Cli.Core.Services.Analytics;

public class AnalyticsService : ExecutionStatusProviderBase, IAnalyticsService
{
  private readonly SourceCache<CheckoutInfoData, string> _checkouts = new(_ => _.Id);
  private readonly Contracts.Analytics.Analytics.AnalyticsClient _analyticsClient;
  private readonly ConcurrentDictionary<int, Lazy<Task<CheckoutInfoPagedList>>> _pages = new();
  private readonly BehaviorSubject<AnalyticsSummary> _analytics = new(new AnalyticsSummary());

  public AnalyticsService(Contracts.Analytics.Analytics.AnalyticsClient analyticsClient)
  {
    _analyticsClient = analyticsClient;
    Checkouts = _checkouts.AsObservableCache();
    AnalyticsSummary = _analytics.AsObservable();
  }

  public async ValueTask<Result> InitializeAsync(CancellationToken ct = default)
  {
    await FetchPage(0, ct);

    var timeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
    var r = await _analyticsClient.GetSummaryAsync(new AnalyticsSummaryRequest
      {
        TimeZone = timeZone.Id
      }, cancellationToken: ct)
      .TrackProgress(FetchingTracker);

    _analytics.OnNext(r);
    return Result.Success();
  }

  public void ResetCache()
  {
    _pages.Clear();
    _checkouts.Clear();
  }

  public IObservableCache<CheckoutInfoData, string> Checkouts { get; }
  public IObservable<AnalyticsSummary> AnalyticsSummary { get; }

  public async ValueTask<CheckoutInfoPagedList> FetchPage(int pageIdx, CancellationToken ct = default)
  {
    return await _pages.GetOrAdd(pageIdx, static (ix, self) => new Lazy<Task<CheckoutInfoPagedList>>(async () =>
    {
      var page = await self._analyticsClient.GetCheckoutInfoPageAsync(new CheckoutInfoPageRequest
        {
          Limit = 100,
          EndAt = Timestamp.FromDateTimeOffset(DateTimeOffset.MaxValue),
          StartAt = Timestamp.FromDateTimeOffset(DateTimeOffset.MinValue),
          PageIndex = ix
        })
        .TrackProgress(self.FetchingTracker);

      self._checkouts.AddOrUpdate(page.Content);
      return page;
    }), this).Value;
  }
}