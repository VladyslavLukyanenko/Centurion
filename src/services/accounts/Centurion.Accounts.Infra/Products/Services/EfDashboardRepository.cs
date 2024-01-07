using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Config;
using Centurion.Accounts.Core.Products.Services;
using Centurion.Accounts.Infra.Repositories;

namespace Centurion.Accounts.Infra.Products.Services;

public class EfDashboardRepository : EfSoftRemovableCrudRepository<Dashboard, Guid>, IDashboardRepository
{
  private readonly IQueryable<Member> _members;
  private readonly DashboardsConfig _config;

  public EfDashboardRepository(DbContext context, IUnitOfWork unitOfWork, DashboardsConfig config)
    : base(context, unitOfWork)
  {
    _config = config;
    _members = Context.Set<Member>();
  }

  public async ValueTask<Dashboard?> GetByRawLocationAsync(string rawLocation, CancellationToken ct = default)
  {
    var modes = HostingConfig.ResolvePossibleModes(rawLocation, _config);
    if (modes.IsFailure)
    {
      return null;
    }

    var normalizedModes = modes.Value.Select(m => $"{(int) m.Key}__{m.Value.ToLower()}");
    return await DataSource.FirstOrDefaultAsync(
      d => normalizedModes.Contains(d.HostingConfig.Mode + "__" + d.HostingConfig.DomainName.ToLower()), ct);
  }

  public async ValueTask<bool> AlreadyJoinedAsync(Guid dashboardId, long userId, CancellationToken ct = default)
  {
    return await _members.AnyAsync(_ => _.DashboardId == dashboardId && _.UserId == userId, ct);
  }

  public async ValueTask<Member> AddMemberAsync(Member member, CancellationToken ct = default)
  {
    var e = await Context.AddAsync(member, ct);
    return e.Entity;
  }

  public async ValueTask<IList<AccessibleDashboard>> GetAccessibleDashboardsAsync(long userId,
    CancellationToken ct = default)
  {
    var ownDashboards = DataSource.Where(_ => _.OwnerId == userId)
      .Select(d => new AccessibleDashboard
      {
        Id = d.Id,
        IsProperty = true
      });

    var members = _members.Where(_ => _.UserId == userId)
      .Select(_ => new AccessibleDashboard
      {
        Id = _.DashboardId,
        IsProperty = false
      });

    return await ownDashboards.Union(members)
      .ToListAsync(ct);
  }

  public async ValueTask<Dashboard?> GetByOwnerIdAsync(long userId, CancellationToken ct = default)
  {
    return await DataSource.FirstOrDefaultAsync(_ => _.OwnerId == userId, ct);
  }
}