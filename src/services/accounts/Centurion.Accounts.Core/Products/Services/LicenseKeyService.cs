using Centurion.Accounts.Core.Identity;
using CSharpFunctionalExtensions;
using NodaTime;

namespace Centurion.Accounts.Core.Products.Services;

public class LicenseKeyService : ILicenseKeyService
{
  private readonly ILicenseKeyRepository _licenseKeyRepository;
  private readonly IPlanRepository _planRepository;
  private readonly IStripeGateway _stripeGateway;
  private readonly IDiscordService _discordService;
  private readonly IStatsService _statsService;
  private readonly ILicenseKeyGenerationStrategyProvider _keyGenerationStrategyProvider;
  private readonly ILicenseKeyScheduler _licenseKeyScheduler;
  private readonly IDashboardRepository _dashboardRepository;

  public LicenseKeyService(ILicenseKeyRepository licenseKeyRepository, IPlanRepository planRepository,
    IStripeGateway stripeGateway, IDiscordService discordService, IStatsService statsService,
    ILicenseKeyGenerationStrategyProvider keyGenerationStrategyProvider, ILicenseKeyScheduler licenseKeyScheduler,
    IDashboardRepository dashboardRepository)
  {
    _licenseKeyRepository = licenseKeyRepository;
    _planRepository = planRepository;
    _stripeGateway = stripeGateway;
    _discordService = discordService;
    _statsService = statsService;
    _keyGenerationStrategyProvider = keyGenerationStrategyProvider;
    _licenseKeyScheduler = licenseKeyScheduler;
    _dashboardRepository = dashboardRepository;
  }

  public async ValueTask<Result<LicenseKey>> PurchaseAsync(User user, Plan plan, Release release, string keyValue,
    string? paymentIntent = null, string? subscriptionId = null, CancellationToken ct = default)
  {
    var licenseKeyCreateResult = LicenseKey.Purchase(plan, user.Id, keyValue, release, subscriptionId, paymentIntent);
    if (licenseKeyCreateResult.IsFailure)
    {
      return licenseKeyCreateResult;
    }

    var licenseKey = await _licenseKeyRepository.CreateAsync(licenseKeyCreateResult.Value, ct);
    if (licenseKey.IsTrial())
    {
      await _licenseKeyScheduler.ScheduleKeyRemovalAsync(licenseKey, ct);
    }

    return licenseKey;
  }

  public async ValueTask GenerateKeys(Plan plan, GenerateLicenseKeysCommand cmd, CancellationToken ct = default)
  {
    var keys = new List<LicenseKey>((int) cmd.Quantity);
    var keyGenerator = _keyGenerationStrategyProvider.GetGeneratorFor(plan!.LicenseKeyConfig);
    Instant? trialEndsAt = null;
    if (cmd.TrialDaysCount > 0)
    {
      trialEndsAt = SystemClock.Instance.GetCurrentInstant() + Duration.FromDays(cmd.Quantity);
    }

    Instant? unbindableAfter = cmd.AllowUnbinding ? SystemClock.Instance.GetCurrentInstant() : null;
    Instant? expiresAt = cmd.IsLifetime ? null : plan.CalculateKeyExpiry();
    for (int i = 0; i < cmd.Quantity; i++)
    {
      var value = await keyGenerator.GenerateValueAsync(plan, ct);
      var key = LicenseKey.Create(plan, value, unbindableAfter, expiresAt, trialEndsAt);
      keys.Add(key);
    }

    await _licenseKeyRepository.CreateAsync(keys, ct);
  }

  public async ValueTask<Result> UnbindAsync(LicenseKey key, CancellationToken ct = default)
  {
    if (!key.CanBeUnbound())
    {
      return Result.Failure("This license isn't unbindable");
    }

    var dashboard = await _dashboardRepository.GetByIdAsync(key.DashboardId, ct);
    Plan? plan = await _planRepository.GetByIdAsync(key.PlanId, ct);
    // todo: check if user has other keys with same plan and skip removal in this case + skip product roles remove in case if different key present
    await _discordService.RemoveRolesByKeyAsync(dashboard!, key, plan!, ct);
    if (!plan!.IsLifetimeLimited())
    {
      await _stripeGateway.CancelSubscriptionAsync(key.DashboardId, key.SubscriptionId!, ct);
    }

    await _statsService.RemoveByLicenseKeyIdAsync(key.Id, ct);

    var strategy = _keyGenerationStrategyProvider.GetGeneratorFor(plan.LicenseKeyConfig);
    string newValue = await strategy.GenerateValueAsync(plan, ct);
    return key.Unbind(newValue)
      .OnSuccessTry(() => _licenseKeyRepository.Update(key));
  }

  public async ValueTask<Result> BindAsync(LicenseKey key, User user, string? discordToken,
    CancellationToken ct = default)
  {
    var plan = await _planRepository.GetByIdAsync(key.PlanId, ct);
    string? subscriptionId = null;
    if (!key.IsLifetime())
    {
      if (string.IsNullOrEmpty(user.StripeCustomerId))
      {
        return Result.Failure("No stripe customer id associated with user");
      }

      var startResult =
        await _stripeGateway.StartSubscriptionAsync(user.StripeCustomerId, plan!, key.CalculateAbsoluteExpiry(), ct);
      if (startResult.IsFailure)
      {
        return startResult;
      }

      subscriptionId = startResult.Value;
    }

    return await key.BindToUser(user.Id, subscriptionId)
      .OnSuccessTry(async () =>
      {
        _licenseKeyRepository.Update(key);
        if (!string.IsNullOrEmpty(discordToken))
        {
          var dashboard = await _dashboardRepository.GetByIdAsync(key.DashboardId, ct);
          await _discordService.JoinToGuildAsync(dashboard!, discordToken, ct);
        }
      });
  }

  public async ValueTask<Result> SuspendAsync(LicenseKey licenseKey, CancellationToken ct = default)
  {
    return await licenseKey.Suspend()
      .OnSuccessTry(async () =>
      {
        if (licenseKey.HasSubscription())
        {
          await _stripeGateway.PauseSubscriptionAsync(licenseKey.DashboardId, licenseKey.SubscriptionId!, ct);
        }
      });
  }

  public async ValueTask<Result> ResumeAsync(LicenseKey licenseKey, CancellationToken ct = default)
  {
    return await licenseKey.Resume()
      .OnSuccessTry(async () =>
      {
        if (licenseKey.HasSubscription())
        {
          await _stripeGateway.ResumeSubscriptionAsync(licenseKey.DashboardId, licenseKey.SubscriptionId!, ct);
        }
      });
  }

  public async ValueTask<Result> SuspendAllAsync(long planId, CancellationToken ct = default)
  {
    return await ProcessAllRenewalsByPlanIdAsync(planId, IsSuspendable, SuspendAsync, ct);
    bool IsSuspendable(LicenseKey licenseKey) => !licenseKey.IsExpired() && !licenseKey.IsSuspended();
  }

  public async ValueTask<Result> ResumeAllAsync(long planId, CancellationToken ct = default)
  {
    return await ProcessAllRenewalsByPlanIdAsync(planId, CanBeResumed, ResumeAsync, ct);
    bool CanBeResumed(LicenseKey licenseKey) => licenseKey.IsSuspended();
  }

  public async ValueTask<Result> CancelSubscriptionAsync(string subscriptionId, CancellationToken ct = default)
  {
    var key = await _licenseKeyRepository.GetBySubscriptionIdAsync(subscriptionId, ct);
    if (key == null)
    {
      return Result.Failure("Can't find key");
    }

    return key.CancelSubscription()
      .OnSuccessTry(() => _licenseKeyRepository.Update(key));
  }

  public async ValueTask<Result> TryResumeSubscriptionAsync(string subscriptionId, CancellationToken ct = default)
  {
    var key = await _licenseKeyRepository.GetBySubscriptionIdAsync(subscriptionId, ct);
    if (key == null)
    {
      return Result.Failure("Can't find key");
    }

    return key.ResumeSubscription()
      .OnSuccessTry(() => _licenseKeyRepository.Update(key));
  }

  public async ValueTask<Result> UpdateAsync(LicenseKey licenseKey, UpdateLicenseKeyCommand cmd,
    CancellationToken ct = default)
  {
    Plan? plan = null;
    if (licenseKey.IsUnbindable() != cmd.IsUnbindable || licenseKey.IsLifetime() != cmd.IsLifetime)
    {
      plan = await _planRepository.GetByIdAsync(cmd.PlanId, ct);
      if (plan == null)
      {
        return Result.Failure<Plan>("Can't find plan");
      }
    }

    if (licenseKey.IsLifetime() != cmd.IsLifetime)
    {
      if (!cmd.IsLifetime)
      {
        licenseKey.Renew(plan!.CalculatePossibleKeyExpiry());
      }
      else
      {
        licenseKey.MakeNonExpirable();
      }
    }

    if (licenseKey.IsUnbindable() != cmd.IsUnbindable)
    {
      if (!cmd.IsUnbindable)
      {
        licenseKey.MakeNotUnbindable();
      }
      else
      {
        licenseKey.ResetUnbindDelay(plan!.CalculatePossibleUnbindableAfter());
      }
    }

    licenseKey.PlanId = cmd.PlanId;
    licenseKey.Reason = cmd.Notes;
    _licenseKeyRepository.Update(licenseKey);

    return Result.Success();
  }

  public async ValueTask<Result> RemoveKeyAsync(LicenseKey licenseKey, CancellationToken ct = default)
  {
    if (licenseKey.CanBeUnbound())
    {
      var result = await UnbindAsync(licenseKey, ct);
      if (result.IsFailure)
      {
        return result;
      }
    }

    _licenseKeyRepository.Remove(licenseKey);
    return Result.Success();
  }

  public async ValueTask<Result> RenewAsync(string subscriptionId, CancellationToken ct = default)
  {
    var licenseKey = await _licenseKeyRepository.GetBySubscriptionIdAsync(subscriptionId, ct);
    if (licenseKey == null)
    {
      return Result.Failure("Can't find key");
    }

    var plan = await _planRepository.GetByIdAsync(licenseKey.PlanId, ct);
    var expiry = plan?.CalculateKeyExpiry();
    if (expiry == null)
    {
      return Result.Failure("Plan is lifetime");
    }

    licenseKey.Renew(expiry.Value);
    _licenseKeyRepository.Update(licenseKey);
    return Result.Success();
  }

  private async ValueTask<Result> ProcessAllRenewalsByPlanIdAsync(long planId, Predicate<LicenseKey> isAcceptable,
    Func<LicenseKey, CancellationToken, ValueTask<Result>> processor, CancellationToken ct = default)
  {
    var keys = await _licenseKeyRepository.GetAllByPlanIdAsync(planId, ct);
    foreach (var licenseKey in keys)
    {
      if (!isAcceptable(licenseKey))
      {
        continue;
      }

      var result = await processor(licenseKey, ct);
      if (result.IsFailure)
      {
        return result;
      }
    }

    return Result.Success();
  }
}