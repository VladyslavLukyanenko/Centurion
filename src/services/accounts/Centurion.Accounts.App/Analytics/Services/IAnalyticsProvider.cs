using Centurion.Accounts.App.Analytics.Model;
using CSharpFunctionalExtensions;

namespace Centurion.Accounts.App.Analytics.Services;

public interface IAnalyticsProvider
{
  ValueTask<Result<GeneralAnalytics>> GetGeneralAnalyticsAsync(Guid dashboardId, GeneralAnalyticsRequest request,
    CancellationToken ct = default);
}