using Centurion.Accounts.Core.Products;
using Stripe;

namespace Centurion.Accounts.Infra.Services;

public interface IStripeClientFactory
{
  ValueTask<IStripeClient> CreateClientAsync(Guid dashboardId, CancellationToken ct = default);
  IStripeClient CreateClient(Dashboard dashboard);
}