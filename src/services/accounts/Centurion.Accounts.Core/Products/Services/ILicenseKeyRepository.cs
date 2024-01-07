using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.Products.Services;

public interface ILicenseKeyRepository : ICrudRepository<LicenseKey>
{
  ValueTask<LicenseKey?> GetByValueAsync(string value, CancellationToken ct = default);
  ValueTask<IList<LicenseKey>> GetAllByPlanIdAsync(long planId, CancellationToken ct = default);
  ValueTask<LicenseKey?> GetBySubscriptionIdAsync(string subscriptionId, CancellationToken ct = default);
  ValueTask<bool> ExistsWithValueAsync(string keyValue, CancellationToken ct = default);
  ValueTask<bool> ExistsForPlanAsync(long userId, long planId, IReadOnlyCollection<long> exceptLicenseKeyIds, CancellationToken ct = default);
}