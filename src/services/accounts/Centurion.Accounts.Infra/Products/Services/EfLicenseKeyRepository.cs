using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.Events;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Events.LicenseKeys;
using Centurion.Accounts.Core.Products.Services;
using Centurion.Accounts.Infra.Repositories;

namespace Centurion.Accounts.Infra.Products.Services;

public class EfLicenseKeyRepository : EfSoftRemovableCrudRepository<LicenseKey, long>, ILicenseKeyRepository
{
  public EfLicenseKeyRepository(DbContext context, IUnitOfWork unitOfWork)
    : base(context, unitOfWork)
  {
  }

  public async ValueTask<LicenseKey?> GetByValueAsync(string value, CancellationToken ct = default)
  {
    return await DataSource.FirstOrDefaultAsync(_ => _.Value == value, ct);
  }

  public async ValueTask<IList<LicenseKey>> GetAllByPlanIdAsync(long planId, CancellationToken ct = default)
  {
    return await DataSource.Where(_ => _.PlanId == planId).ToArrayAsync(ct);
  }

  public async ValueTask<LicenseKey?> GetBySubscriptionIdAsync(string subscriptionId, CancellationToken ct = default)
  {
    return await DataSource.FirstOrDefaultAsync(_ => _.SubscriptionId == subscriptionId, ct);
  }

  public async ValueTask<bool> ExistsWithValueAsync(string keyValue, CancellationToken ct = default)
  {
    return await DataSource.AnyAsync(_ => _.Value == keyValue, ct);
  }

  public async ValueTask<bool> ExistsForPlanAsync(long userId, long planId,
    IReadOnlyCollection<long> exceptLicenseKeyIds, CancellationToken ct = default)
  {
    var query = DataSource;
    if (exceptLicenseKeyIds.Any())
    {
      query = query.Where(_ => !exceptLicenseKeyIds.Contains(_.Id));
    }

    return await query.AnyAsync(_ => _.UserId == userId && _.PlanId == planId, ct);
  }

  protected override DomainEvent CreateRemovedEvent(LicenseKey entity) => new LicenseKeyRemoved(entity);
}