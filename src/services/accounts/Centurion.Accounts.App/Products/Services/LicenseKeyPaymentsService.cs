using CSharpFunctionalExtensions;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Identity.Services;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;

namespace Centurion.Accounts.App.Products.Services;

public class LicenseKeyPaymentsService : ILicenseKeyPaymentsService
{
  private readonly IPlanRepository _planRepository;
  private readonly IStripeGateway _stripeGateway;
  private readonly ILicenseKeyService _licenseKeyService;
  private readonly IDiscordService _discordService;
  private readonly ILicenseKeyGenerationStrategyProvider _keyGenerationStrategyProvider;
  private readonly IUserRepository _userRepository;

  public LicenseKeyPaymentsService(IPlanRepository planRepository, IStripeGateway stripeGateway,
    ILicenseKeyService licenseKeyService, IDiscordService discordService,
    ILicenseKeyGenerationStrategyProvider keyGenerationStrategyProvider, IUserRepository userRepository)
  {
    _planRepository = planRepository;
    _stripeGateway = stripeGateway;
    _licenseKeyService = licenseKeyService;
    _discordService = discordService;
    _keyGenerationStrategyProvider = keyGenerationStrategyProvider;
    _userRepository = userRepository;
  }

  public async ValueTask<Result> ProcessPaymentAsync(Dashboard dashboard, long planId, Release release, string intent,
    string customer, User user, string discordToken, CancellationToken ct = default)
  {
    Plan? plan = await _planRepository.GetByIdAsync(planId, ct);
    if (plan == null)
    {
      return Result.Failure("Plan not found");
    }


    user.StripeCustomerId = customer;
    _userRepository.Update(user);

    string? subscriptionId = null;
    if (plan.IsLifetimeLimited() && !plan.IsTrial)
    {
      if (string.IsNullOrEmpty(customer))
      {
        return Result.Failure("Can't start subscription with empty stripe customer id");
      }

      var startResult =
        await _stripeGateway.StartSubscriptionAsync(customer, plan, plan.CalculateKeyExpiry(), ct);
      if (startResult.IsFailure)
      {
        return startResult;
      }

      subscriptionId = startResult.Value;
    }

    return await RegisterLicenseKeyForUserAsync(dashboard, user, plan, release, discordToken, intent, subscriptionId,
      ct);
  }

  public async ValueTask<Result> AcquireTrialKeyAsync(Dashboard dashboard, long planId, Release release, User user,
    string discordToken, CancellationToken ct = default)
  {
    Plan? plan = await _planRepository.GetByIdAsync(planId, ct);
    if (plan == null)
    {
      return Result.Failure("Plan not found");
    }

    if (!plan.IsTrial)
    {
      return Result.Failure("This product plan doesn't support non trial keys");
    }

    return await RegisterLicenseKeyForUserAsync(dashboard, user, plan, release, discordToken, ct: ct);
  }

  private async ValueTask<Result> RegisterLicenseKeyForUserAsync(Dashboard dashboard, User user, Plan plan,
    Release release, string discordToken, string? paymentIntent = null, string? subscriptionId = null,
    CancellationToken ct = default)
  {
    var strategy = _keyGenerationStrategyProvider.GetGeneratorFor(plan.LicenseKeyConfig);
    var keyValue = await strategy.GenerateValueAsync(plan, ct);
    return await _licenseKeyService.PurchaseAsync(user, plan, release, keyValue, paymentIntent, subscriptionId, ct)
      .AsTask()
      .OnSuccessTry(async _ => await _discordService.JoinToGuildAsync(dashboard, discordToken, ct));
  }
}