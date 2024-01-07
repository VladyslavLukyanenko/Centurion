using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;
using CSharpFunctionalExtensions;
using Stripe;

namespace Centurion.Accounts.Services.Stripe;

public class TryResumeSubscriptionOnCustomerSubscriptionUpdatedWebHookHandler : IStripeWebHookHandler
{
  private readonly ILicenseKeyService _licenseKeyService;

  public TryResumeSubscriptionOnCustomerSubscriptionUpdatedWebHookHandler(ILicenseKeyService licenseKeyService)
  {
    _licenseKeyService = licenseKeyService;
  }

  public bool CanHandle(string eventType) => eventType == Events.CustomerSubscriptionUpdated;

  public async ValueTask<Result> HandleAsync(Event @event, Dashboard dashboard, CancellationToken ct = default)
  {
    var subscription = (Subscription) @event.Data.Object;
    if (!subscription.CancelAtPeriodEnd)
    {
      await _licenseKeyService.TryResumeSubscriptionAsync(subscription.Id, ct);
    }

    return Result.Success();
  }
}