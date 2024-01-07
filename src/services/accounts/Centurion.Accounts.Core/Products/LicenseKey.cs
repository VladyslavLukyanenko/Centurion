using Centurion.Accounts.Core.Products.Events.LicenseKeys;
using CSharpFunctionalExtensions;
using NodaTime;

namespace Centurion.Accounts.Core.Products;

public class LicenseKey : DashboardBoundEntity
{
  private const string DefaultGenerationReason = "Purchase";
  private List<SuspensionLicensePeriod> _suspensions = new();

  // ReSharper disable once UnusedMember.Local
  private LicenseKey()
  {
  }

  private LicenseKey(Guid dashboardId, long? userId, long planId, long? releaseId, Instant? expiry,
    string? subscriptionId, string? paymentIntent, string value, string? reason, Instant? trialEndsAt,
    Instant? unbindableAfter)
    : base(dashboardId)
  {
    UserId = userId;
    PlanId = planId;
    ReleaseId = releaseId;
    Expiry = expiry;
    SubscriptionId = subscriptionId;
    PaymentIntent = paymentIntent;
    Value = value;
    Reason = reason;
    TrialEndsAt = trialEndsAt;
    UnbindableAfter = unbindableAfter;
    if (!string.IsNullOrEmpty(SubscriptionId))
    {
      SubscribedAt = SystemClock.Instance.GetCurrentInstant();
    }

    //
    // if (trialEndsAt.HasValue)
    // {
    //   RemoveAt(trialEndsAt.Value);
    // }
  }

  public static Result<LicenseKey> Purchase(Plan plan, long userId, string value, Release? release = null,
    string? subscriptionId = null, string? paymentIntent = null, string reason = DefaultGenerationReason)
  {
    // if (string.IsNullOrEmpty(subscriptionId) && plan.IsTrial)
    // {
    //   return Result.Failure<LicenseKey>("No subscription id provided for trial plan");
    // }

    Instant? expiry = null;
    if (!string.IsNullOrEmpty(subscriptionId))
    {
      expiry = plan.CalculateKeyExpiry();
    }

    Instant? trialEndsAt = null;
    if (plan.IsTrial)
    {
      trialEndsAt = SystemClock.Instance.GetCurrentInstant() + plan.TrialPeriod;
    }

    var licenseKey = new LicenseKey(plan.DashboardId, userId, plan.Id, release?.Id, expiry, subscriptionId,
      paymentIntent, value, reason, trialEndsAt, plan.CalculateUnbindableAfter());
    licenseKey.AddDomainEvent(new LicenseKeyPurchased(licenseKey, userId, trialEndsAt));

    return licenseKey;
  }

  public static LicenseKey Create(Plan plan, string value, Instant? unbindableAfter, Instant? expiresAt,
    Instant? trialEndsAt)
  {
    return new(plan.DashboardId, null, plan.Id, null, expiresAt, null, null, value, "Generated", trialEndsAt,
      unbindableAfter);
    // licenseKey.AddDomainEvent(new LicenseKeyPurchased(licenseKey, userId, trialEndsAt));
  }

  public bool CanBeUnbound() =>
    UserId.HasValue && UnbindableAfter.HasValue && UnbindableAfter <= SystemClock.Instance.GetCurrentInstant();

  public bool IsUnbindable() => UnbindableAfter.HasValue;

  public void RefreshActivity() => LastAuthRequest = SystemClock.Instance.GetCurrentInstant();
  public void ResetSession() => SessionId = null;

  public void BindSessionIfEmpty(string hwid)
  {
    if (!string.IsNullOrEmpty(SessionId))
    {
      return;
    }

    SessionId = hwid;
  }

  public bool IsSessionValid(string hwid) =>
    !string.IsNullOrEmpty(hwid) && (string.IsNullOrEmpty(SessionId) || SessionId == hwid);

  public bool IsExpired()
  {
    var absoluteExpiry = CalculateAbsoluteExpiry();
    return absoluteExpiry.HasValue && absoluteExpiry < SystemClock.Instance.GetCurrentInstant();
  }

  public Instant? CalculateAbsoluteExpiry()
  {
    if (!Expiry.HasValue || IsSuspended())
    {
      return null;
    }

    var totalSuspensionTime = Suspensions.Where(_ => _.End.HasValue).Select(_ => _.End!.Value - _.Start)
      .Aggregate(Duration.Zero, (l, r) => l + r);
    return Expiry.Value + totalSuspensionTime;
  }

  public bool IsSuspended() =>
    Suspensions.Count > 0
    && Suspensions.Any(_ => !_.End.HasValue || _.End > SystemClock.Instance.GetCurrentInstant());

  public Result Unbind(string newValue)
  {
    if (!CanBeUnbound())
    {
      return Result.Failure("Key is not unbindable");
    }

    var licenseKeyUnbound = new LicenseKeyUnbound(this, SubscriptionId, SessionId, UserId!.Value, Value);
    Value = newValue;
    UserId = null;
    SessionId = null;
    SubscriptionId = null;
    AddDomainEvent(licenseKeyUnbound);

    return Result.Success();
  }

  public Result BindToUser(long userId, string? subscriptionId)
  {
    if (IsSuspended())
    {
      return Result.Failure<LicenseKey>("Key is suspended.");
    }

    if (IsExpired())
    {
      return Result.Failure<LicenseKey>("Key is expired.");
    }

    if (UserId.HasValue)
    {
      return Result.Failure("This key is already bound to someone else");
    }

    if (UserId == userId)
    {
      return Result.Failure<LicenseKey>("This key is already bound to your account.");
    }

    UserId = userId;
    SubscriptionId = subscriptionId;
    AddDomainEvent(new LicenseKeyBound(this, userId, subscriptionId));

    return Result.Success();
  }

  public bool IsLifetime() => !Expiry.HasValue;

  public bool IsUnbound() => !UserId.HasValue;

  public Result Suspend()
  {
    if (IsExpired())
    {
      return Result.Failure("Key is expired");
    }

    if (IsSuspended())
    {
      return Result.Failure("Already suspended");
    }

    _suspensions.Add(SuspensionLicensePeriod.CreateStarted());
    return Result.Success();
  }

  public bool HasSubscription() => !string.IsNullOrEmpty(SubscriptionId);

  public Result Resume()
  {
    if (!IsSuspended())
    {
      return Result.Failure("License key is not suspended");
    }

    var currentPeriod = _suspensions.Single(_ => _.IsInProgress());
    return currentPeriod.Finish();
  }

  public void Renew(Instant expiry)
  {
    Expiry = expiry;
    ResumeSubscription();
  }

  public bool IsTrial() => TrialEndsAt.HasValue;

  public Result CancelSubscription()
  {
    if (SubscriptionCancelledAt.HasValue)
    {
      return Result.Failure("Subscription already cancelled");
    }

    SubscriptionCancelledAt = SystemClock.Instance.GetCurrentInstant();
    return Result.Success();
  }

  public Result ResumeSubscription()
  {
    if (IsExpired())
    {
      return Result.Failure("Already expired");
    }

    if (!SubscriptionCancelledAt.HasValue)
    {
      return Result.Failure("Subscription was not cancelled");
    }

    SubscriptionCancelledAt = null;
    return Result.Success();
  }

  internal void MakeNonExpirable()
  {
    Expiry = null;
  }

  internal void MakeNotUnbindable()
  {
    UnbindableAfter = null;
  }

  internal void ResetUnbindDelay(Instant unbindableAfter)
  {
    UnbindableAfter = unbindableAfter;
  }

  public long? UserId { get; private set; }
  public long PlanId { get; internal set; }
  public long? ReleaseId { get; private set; }

  public Instant? Expiry { get; private set; }
  public Instant? LastAuthRequest { get; private set; }

  public string? SessionId { get; private set; }
  public string? SubscriptionId { get; private set; }
  public Instant? SubscribedAt { get; private set; }
  public Instant? SubscriptionCancelledAt { get; private set; }
  public string? PaymentIntent { get; private set; }
  public string Value { get; private set; } = null!;
  public string? Reason { get; internal set; }

  public Instant? TrialEndsAt { get; private set; }
  public Instant? UnbindableAfter { get; private set; }
  public IReadOnlyList<SuspensionLicensePeriod> Suspensions => _suspensions.AsReadOnly();

  public Result Prolong(Duration prolongFor)
  {
    if (IsLifetime())
    {
      return Result.Failure("License key is lifetime");
    }


    Expiry = Expiry!.Value + prolongFor;
    return Result.Success();
  }
}