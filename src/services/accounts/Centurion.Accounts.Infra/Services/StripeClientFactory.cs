using Centurion.Accounts.Core;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;
using Stripe;

namespace Centurion.Accounts.Infra.Services;

public class StripeClientFactory : IStripeClientFactory
{
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly IDashboardRepository _dashboardRepository;

  public StripeClientFactory(IHttpClientFactory httpClientFactory, IDashboardRepository dashboardRepository)
  {
    _httpClientFactory = httpClientFactory;
    _dashboardRepository = dashboardRepository;
  }

  public async ValueTask<IStripeClient> CreateClientAsync(Guid dashboardId, CancellationToken ct = default)
  {
    var dashboard = await _dashboardRepository.GetByIdAsync(dashboardId, ct)
                    ?? throw new CoreException("Can't find dashboard with id " + dashboardId);
    return CreateClient(dashboard);
  }

  public IStripeClient CreateClient(Dashboard dashboard)
  {
    var client = _httpClientFactory.CreateClient();
    return new StripeClient(httpClient: new SystemNetHttpClient(client), apiKey: dashboard.StripeConfig.ApiKey);
  }
}