using CSharpFunctionalExtensions;
using NodaTime;

namespace Centurion.Accounts.Core.Products.Services;

public interface IStripeGateway
{
  ValueTask<Result<string>> StartSubscriptionAsync(string customerStripeId, Plan plan, Instant? expiry,
    CancellationToken ct = default);

  ValueTask CancelSubscriptionAsync(Guid dashboardId, string subscriptionId, CancellationToken ct = default);
  ValueTask PauseSubscriptionAsync(Guid dashboardId, string subscriptionId, CancellationToken ct = default);
  ValueTask ResumeSubscriptionAsync(Guid dashboardId, string subscriptionId, CancellationToken ct = default);

  ValueTask<Result<string>> OpenBillingPortalSessionAsync(string customerId, Dashboard dashboard,
    CancellationToken ct = default);

  ValueTask<Result<StripeSessionData>> GetSessionDataAsync(Guid dashboardId, string sessionId,
    CancellationToken ct = default);

  ValueTask<Result<string>> CreatePaymentSessionAsync(Release release, Dashboard dashboard, string? stripeCustomerId,
    CancellationToken ct = default);

  ValueTask<Result<bool>> IsCapturedAsync(Guid dashboardId, string intent, CancellationToken ct = default);
}