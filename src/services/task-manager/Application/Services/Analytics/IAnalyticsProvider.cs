using Centurion.Contracts.Analytics;
using Centurion.SeedWork.Collections;
using NodaTime;

namespace Centurion.TaskManager.Application.Services.Analytics;

public interface IAnalyticsProvider
{
  ValueTask<IPagedList<CheckoutInfoData>> GetCheckoutsPage(string userId, ICheckoutInfoPageRequest request,
    CancellationToken ct = default);

  ValueTask<AnalyticsSummary> GetSummary(string userId, DateTimeZone timeZone, CancellationToken ct = default);
}