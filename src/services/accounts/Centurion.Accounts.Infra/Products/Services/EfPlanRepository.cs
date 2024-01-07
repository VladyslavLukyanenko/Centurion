using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;
using Centurion.Accounts.Infra.Repositories;

namespace Centurion.Accounts.Infra.Products.Services;

public class EfPlanRepository : EfSoftRemovableCrudRepository<Plan, long>, IPlanRepository
{
  public EfPlanRepository(DbContext context, IUnitOfWork unitOfWork)
    : base(context, unitOfWork)
  {
  }

  public async ValueTask<(ulong GuidId, IEnumerable<ulong> RoleIds)> GetDiscordRolesInfoAsync(long planId,
    CancellationToken ct = default)
  {
    var roleIds = from plan in DataSource
      join pr in Context.Set<Dashboard>() on plan.DashboardId equals pr.Id
      where plan.Id == planId
      select new
      {
        GuidId = pr.DiscordConfig.GuildId,
        ProductRoleId = pr.DiscordConfig.RoleId,
        PlanRoleId = plan.DiscordRoleId
      };

    var r = await roleIds.FirstOrDefaultAsync(ct);
    return (r.GuidId, new[] {r.ProductRoleId, r.PlanRoleId});
  }

  public async ValueTask<Plan?> GetByPasswordAsync(Guid dashboardId, string password, CancellationToken ct = default)
  {
    var releases = Context.Set<Release>().WhereNotRemoved();
    var query = from plan in DataSource
      join release in releases on plan.Id equals release.PlanId
      where plan.DashboardId == dashboardId && release.Password == password
      select plan;

    return await query.FirstOrDefaultAsync(ct);
  }
}