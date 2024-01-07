using CSharpFunctionalExtensions;
using NodaTime;
using Centurion.Accounts.App;
using Centurion.Accounts.App.Config;
using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.App.Services;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;
using Centurion.Accounts.Infra.Configs;
using Stripe;
using Stripe.Checkout;
using Plan = Centurion.Accounts.Core.Products.Plan;
using SessionCreateOptions = Stripe.BillingPortal.SessionCreateOptions;
using SessionService = Stripe.BillingPortal.SessionService;

namespace Centurion.Accounts.Infra.Services;

public class StripeGateway : IStripeGateway
{
  private readonly StripeGlobalConfig _stripeConfig;
  private readonly IPlanProvider _planProvider;
  private readonly IStripeClientFactory _stripeClientFactory;
  private readonly CommonConfig _commonConfig;
  private readonly IPathsService _pathsService;

  public StripeGateway(StripeGlobalConfig stripeConfig, IPlanProvider planProvider,
    IStripeClientFactory stripeClientFactory, CommonConfig commonConfig, IPathsService pathsService)
  {
    _stripeClientFactory = stripeClientFactory;
    _commonConfig = commonConfig;
    _pathsService = pathsService;
    _stripeConfig = stripeConfig;
    _planProvider = planProvider;
  }

  public async ValueTask<Result<string>> StartSubscriptionAsync(string customerStripeId, Plan plan, Instant? expiry,
    CancellationToken ct = default)
  {
    var options = new SubscriptionCreateOptions
    {
      Customer = customerStripeId,
      TrialEnd = expiry?.ToDateTimeUtc(),
      Items = new List<SubscriptionItemOptions>
      {
        new()
        {
          Price = plan.SubscriptionPlan,
        }
      },
      Metadata = new Dictionary<string, string>
      {
        {"planId", plan.Id.ToString()}
      }
    };

    var subscriptionService = await CreateSubscriptionServiceAsync(plan.DashboardId, ct);
    var subscription = await subscriptionService.CreateAsync(options, cancellationToken: ct);
    return subscription.Id;
  }

  private async Task<SubscriptionService> CreateSubscriptionServiceAsync(Guid dashboardId, CancellationToken ct)
  {
    var stripeClient = await _stripeClientFactory.CreateClientAsync(dashboardId, ct);
    return new SubscriptionService(stripeClient);
  }

  public async ValueTask CancelSubscriptionAsync(Guid dashboardId, string subscriptionId,
    CancellationToken ct = default)
  {
    var subscriptionService = await CreateSubscriptionServiceAsync(dashboardId, ct);
    await subscriptionService.CancelAsync(subscriptionId, cancellationToken: ct);
  }

  public async ValueTask PauseSubscriptionAsync(Guid dashboardId, string subscriptionId,
    CancellationToken ct = default)
  {
    var options = new SubscriptionUpdateOptions
    {
      PauseCollection = new SubscriptionPauseCollectionOptions
      {
        Behavior = "void"
      }
    };

    var subscriptionService = await CreateSubscriptionServiceAsync(dashboardId, ct);
    await subscriptionService.UpdateAsync(subscriptionId, options, cancellationToken: ct);
  }

  public async ValueTask ResumeSubscriptionAsync(Guid dashboardId, string subscriptionId,
    CancellationToken ct = default)
  {
    var options = new SubscriptionUpdateOptions();
    options.AddExtraParam("pause_collection", "");

    var subscriptionService = await CreateSubscriptionServiceAsync(dashboardId, ct);
    await subscriptionService.UpdateAsync(subscriptionId, options, cancellationToken: ct);
  }

  public async ValueTask<Result<string>> OpenBillingPortalSessionAsync(string customerId, Dashboard dashboard,
    CancellationToken ct = default)
  {
    var options = new SessionCreateOptions
    {
      Customer = customerId,
      ReturnUrl = _stripeConfig.GetBillingDashboardReturnUrl(dashboard, _commonConfig)
    };

    var sessionService = new SessionService(_stripeClientFactory.CreateClient(dashboard));
    var session = await sessionService.CreateAsync(options, cancellationToken: ct);
    return session.Url;
  }

  public async ValueTask<Result<StripeSessionData>> GetSessionDataAsync(Guid dashboardId, string sessionId,
    CancellationToken ct = default)
  {
    var stripeClient = await _stripeClientFactory.CreateClientAsync(dashboardId, ct);
    var sessionService = new Stripe.Checkout.SessionService(stripeClient);
    var session = await sessionService.GetAsync(sessionId, cancellationToken: ct);
    if (!session.Metadata.TryGetValue("password", out var password)
        || !session.Metadata.TryGetValue("planId", out var planIdRaw) || !long.TryParse(planIdRaw, out var planId))
    {
      return Result.Failure<StripeSessionData>("Required metadata is empty");
    }

    return new StripeSessionData
    {
      Customer = session.CustomerId,
      Intent = session.PaymentIntentId,
      Password = password,
      PlanId = planId
    };
  }

  public async ValueTask<Result<string>> CreatePaymentSessionAsync(Release release, Dashboard dashboard,
    string? stripeCustomerId, CancellationToken ct = default)
  {
    var plan = await _planProvider.GetByIdAsync(release.PlanId, ct);
    if (plan == null)
    {
      return Result.Failure<string>("Invalid release provided. Plan not found");
    }

    var productPic = _pathsService.GetAbsoluteImageUrl(dashboard.ProductInfo.ImageSrc);
    var sessionService = new Stripe.Checkout.SessionService(_stripeClientFactory.CreateClient(dashboard));
    var session = await sessionService.CreateAsync(new Stripe.Checkout.SessionCreateOptions
    {
      Customer = stripeCustomerId,
      PaymentMethodTypes = new List<string>
      {
        "card"
      },
      SuccessUrl = _stripeConfig.GetPaymentSuccessfulUrl(dashboard, _commonConfig),
      CancelUrl = _stripeConfig.GetPaymentCancelledUrl(dashboard, _commonConfig),
      LineItems = new List<SessionLineItemOptions>
      {
        new()
        {
          Amount = (long) plan.Amount * 100,
          Currency = plan.Currency,
          Description = dashboard.ProductInfo.Description,
          Images = new List<string> {productPic!},
          Name = dashboard.ProductInfo.Name,
          Quantity = 1
        }
      },
      PaymentIntentData = new SessionPaymentIntentDataOptions
      {
        CaptureMethod = "manual"
      },
      Metadata = new Dictionary<string, string>
      {
        {"planId", plan.Id.ToString()},
        {"password", release.Password}
      }
    }, cancellationToken: ct);

    return session.Id;
  }

  public async ValueTask<Result<bool>> IsCapturedAsync(Guid dashboardId, string intent,
    CancellationToken ct = default)
  {
    PaymentIntent? paymentIntent;
    try
    {
      var stripeClient = await _stripeClientFactory.CreateClientAsync(dashboardId, ct);
      var intentsService = new PaymentIntentService(stripeClient);
      paymentIntent = await intentsService.CaptureAsync(intent, cancellationToken: ct);
    }
    catch (StripeException)
    {
      return Result.Failure<bool>("Can't check capture status");
    }

    return paymentIntent.Charges.Data.FirstOrDefault()?.Captured ?? Result.Failure<bool>("Charges list is empty");
  }
}