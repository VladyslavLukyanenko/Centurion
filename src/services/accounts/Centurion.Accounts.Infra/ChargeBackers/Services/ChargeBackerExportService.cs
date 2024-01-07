using CSharpFunctionalExtensions;
using Centurion.Accounts.App.ChargeBackers.Services;
using Centurion.Accounts.Core.ChargeBackers;
using Centurion.Accounts.Core.ChargeBackers.Services;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Infra.Services;
using Stripe.Radar;

namespace Centurion.Accounts.Infra.ChargeBackers.Services;

public class ChargeBackerExportService : IChargeBackerExportService
{
  private const string ClientIpAddressBlocklistAlias = "client_ip_address_blocklist";
  private const string EmailBlocklistAlias = "email_blocklist";
  private const string CardFingerprintBlocklistAlias = "card_fingerprint_blocklist";

  private readonly IStripeClientFactory _stripeClientFactory;
  private readonly IChargeBackerRepository _chargeBackerRepository;

  public ChargeBackerExportService(IStripeClientFactory stripeClientFactory,
    IChargeBackerRepository chargeBackerRepository)
  {
    _stripeClientFactory = stripeClientFactory;
    _chargeBackerRepository = chargeBackerRepository;
  }

  public async ValueTask<Result> ExportAsync(ChargeBacker chargeBacker, Dashboard dashboard,
    CancellationToken ct = default)
  {
    var valueListItemService = new ValueListItemService(_stripeClientFactory.CreateClient(dashboard));
    var valueListService = new ValueListService(_stripeClientFactory.CreateClient(dashboard));
    var result = chargeBacker.Exported();
    if (result.IsFailure)
    {
      return result;
    }

    var lists = await valueListService.ListAsync(new ValueListListOptions
    {
      Alias = EmailBlocklistAlias
    }, cancellationToken: ct);

    foreach (var list in lists.Data)
    {
      switch (list.Alias)
      {
        case ClientIpAddressBlocklistAlias:
        case EmailBlocklistAlias:
          await valueListItemService.CreateAsync(new ValueListItemCreateOptions
          {
            ValueList = list.Id,
            Value = list.Alias == ClientIpAddressBlocklistAlias ? chargeBacker.IpAddress : chargeBacker.Email
          }, cancellationToken: ct);
          break;
        case CardFingerprintBlocklistAlias:
          foreach (var fingerprint in chargeBacker.CardFingerprints)
          {
            await valueListItemService.CreateAsync(new ValueListItemCreateOptions
            {
              ValueList = list.Id,
              Value = fingerprint
            }, cancellationToken: ct);
          }

          break;
        default:
          continue;
      }
    }

    _chargeBackerRepository.Update(chargeBacker);
    return Result.Success();
  }
}