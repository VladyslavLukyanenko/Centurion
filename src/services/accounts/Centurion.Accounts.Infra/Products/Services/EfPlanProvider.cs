using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Infra.Services;

namespace Centurion.Accounts.Infra.Products.Services;

public class EfPlanProvider : DataProvider, IPlanProvider
{
  private readonly IQueryable<Plan> _plans;
  private readonly IMapper _mapper;

  public EfPlanProvider(DbContext context, IMapper mapper)
    : base(context)
  {
    _mapper = mapper;
    _plans = GetAliveDataSource<Plan>();
  }

  public async ValueTask<IList<PlanData>> GetAllAsync(Guid dashboardId, CancellationToken ct = default)
  {
    return await _plans.Where(_ => _.DashboardId == dashboardId)
      .OrderByDescending(_ => _.CreatedAt)
      .ThenBy(_ => _.Description)
      .ProjectTo<PlanData>(_mapper.ConfigurationProvider)
      .ToListAsync(ct);
  }

  public async ValueTask<PlanData?> GetByIdAsync(long planId, CancellationToken ct = default)
  {
    var release = await _plans.FirstOrDefaultAsync(_ => _.Id == planId, ct);
    return _mapper.Map<PlanData>(release);
  }
}