using Centurion.Accounts.Services.Stripe;
using CSharpFunctionalExtensions;
using Centurion.Accounts.Core.ChargeBackers;
using Centurion.Accounts.Core.ChargeBackers.Services;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Infra.Services;
using Stripe;

namespace Centurion.Accounts.ChargeBackers.StripeWebHooks;

public class ChargeDisputeCreatedWebHookHandler : IStripeWebHookHandler
{
  private readonly IChargeBackerRepository _chargeBackerRepository;
  private readonly IStripeClientFactory _stripeClientFactory;

  public ChargeDisputeCreatedWebHookHandler(IChargeBackerRepository chargeBackerRepository,
    IStripeClientFactory stripeClientFactory)
  {
    _chargeBackerRepository = chargeBackerRepository;
    _stripeClientFactory = stripeClientFactory;
  }

  public bool CanHandle(string eventType) => Events.ChargeDisputeCreated == eventType;

  public async ValueTask<Result> HandleAsync(Event @event, Dashboard dashboard, CancellationToken ct = default)
  {
    var dispute = (Dispute) @event.Data.Object;
    var chargeBacker = new ChargeBacker(dispute.Reason, dispute.Evidence.CustomerPurchaseIp,
      dispute.Evidence.CustomerEmailAddress, dashboard.Id);

    var chargesService = new ChargeService(_stripeClientFactory.CreateClient(dashboard));
    var charge = await chargesService.GetAsync(dispute.ChargeId, cancellationToken: ct);

    var paymentMethodsService = new PaymentMethodService(_stripeClientFactory.CreateClient(dashboard));
    var paymentMethods = await paymentMethodsService.ListAsync(new PaymentMethodListOptions
    {
      Customer = charge.CustomerId,
      Type = "card"
    }, cancellationToken: ct);

    chargeBacker.AddCards(paymentMethods.Select(_ => _.Card.Fingerprint));
    await _chargeBackerRepository.CreateAsync(chargeBacker, ct);

    return Result.Success();
  }
}