using CSharpFunctionalExtensions;
using Centurion.Accounts.Core.Products;
using Stripe;

namespace Centurion.Accounts.Services.Stripe;

public interface IStripeWebHookHandler
{
  bool CanHandle(string eventType);
  ValueTask<Result> HandleAsync(Event @event, Dashboard dashboard, CancellationToken ct = default);
}