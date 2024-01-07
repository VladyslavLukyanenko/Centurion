using CSharpFunctionalExtensions;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;
using Stripe;

namespace Centurion.Accounts.Services.Stripe;

public class InvoicePaidWebHookHandler : IStripeWebHookHandler
{
  private readonly ILicenseKeyService _licenseKeyService;

  public InvoicePaidWebHookHandler(ILicenseKeyService licenseKeyService)
  {
    _licenseKeyService = licenseKeyService;
  }

  public bool CanHandle(string eventType) => eventType == Events.InvoicePaid;

  public async ValueTask<Result> HandleAsync(Event @event, Dashboard dashboard, CancellationToken ct = default)
  {
    var invoice = (Invoice) @event.Data.Object;
    var item = invoice.Lines.FirstOrDefault();
    if (item == null)
    {
      return Result.Failure("Not line items found");
    }

    var subscription = item.Subscription;
    return await _licenseKeyService.RenewAsync(subscription, ct);
  }
}