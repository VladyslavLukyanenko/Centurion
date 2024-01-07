using Centurion.Contracts.Analytics;
using DynamicData;

namespace Centurion.Cli.Core.Services.Analytics;

public interface IAnalyticsService : IAppStateHolder
{
  IObservableCache<CheckoutInfoData, string> Checkouts { get; }
  IObservable<AnalyticsSummary> AnalyticsSummary { get; }
  ValueTask<CheckoutInfoPagedList> FetchPage(int pageIdx, CancellationToken ct = default);
}