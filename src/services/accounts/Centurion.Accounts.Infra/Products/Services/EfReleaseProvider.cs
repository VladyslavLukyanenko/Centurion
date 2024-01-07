using AutoMapper;
using AutoMapper.QueryableExtensions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.Core.Collections;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Infra.Services;

namespace Centurion.Accounts.Infra.Products.Services;

public class EfReleaseProvider : DataProvider, IReleaseProvider
{
  private readonly IQueryable<Release> _releases;
  private readonly IQueryable<Plan> _plans;
  private readonly IMapper _mapper;

  public EfReleaseProvider(DbContext context, IMapper mapper)
    : base(context)
  {
    _mapper = mapper;
    _releases = GetAliveDataSource<Release>();
    _plans = GetDataSource<Plan>();
  }

  public async ValueTask<ReleaseData?> GetByIdAsync(Guid dashboardId, long id, CancellationToken ct = default)
  {
    return await _releases.Where(_ => _.DashboardId == dashboardId)
      .ProjectTo<ReleaseData>(_mapper.ConfigurationProvider)
      .FirstOrDefaultAsync(_ => _.Id == id, ct);
  }

  public async ValueTask<IList<ActiveReleaseInfoData>> GetActiveListAsync(Guid dashboardId,
    CancellationToken ct = default)
  {
    return await (from release in _releases
        where release.DashboardId == dashboardId && release.IsActive
        orderby release.Title, release.CreatedAt descending
        select new ActiveReleaseInfoData
        {
          Id = release.Id,
          Title = release.Title,
        })
      .ToListAsync(ct);
  }

  public async ValueTask<IPagedList<ReleaseRowData>> GetPageAsync(Guid dashboardId, ReleasesPageRequest pageRequest,
    CancellationToken ct = default)
  {
    var query = from release in _releases
      join plan in _plans on release.PlanId equals plan.Id
      where release.DashboardId == dashboardId
      select new
      {
        plan, release
      };

    if (pageRequest.PlanId.HasValue)
    {
      query = query.Where(_ => _.plan.Id == pageRequest.PlanId);
    }

    if (pageRequest.Type.HasValue)
    {
      query = query.Where(_ => _.release.Type == pageRequest.Type);
    }

    if (pageRequest.SortBy.HasValue)
    {
      switch (pageRequest.SortBy.Value)
      {
        case ReleasesSortBy.Newest:
          query = query.OrderByDescending(_ => _.release.CreatedAt);
          break;
        case ReleasesSortBy.Oldest:
          query = query.OrderBy(_ => _.release.CreatedAt);
          break;
        case ReleasesSortBy.Stock:
          query = query.OrderByDescending(_ => _.release.Stock)
            .ThenByDescending(_ => _.release.InitialStock);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    return await query.Select(_ =>
        new ReleaseRowData
        {
          Id = _.release.Id,
          Stock = _.release.Stock,
          Title = _.release.Title,
          Type = _.release.Type,
          InitialStock = _.release.InitialStock,
          IsActive = _.release.IsActive,
          PlanDesc = _.plan.Description
        })
      .PaginateAsync(pageRequest, ct);
  }

  public async ValueTask<Maybe<ReleaseStockData?>> GetStockByPasswordAsync(string password,
    CancellationToken ct = default)
  {
    var query = from release in _releases
      join plan in _plans on release.PlanId equals plan.Id
      where release.Password == password && release.Stock > 0
      select new ReleaseStockData
      {
        Price = plan.Amount,
        Currency = plan.Currency,
        IsLifetime = plan.LicenseLife == null,
        LicenseDesc = plan.Description,
        IsTrial = plan.IsTrial,
        Stock = release.Stock,
        DashboardId = release.DashboardId
      };

    return await query.FirstOrDefaultAsync(ct);
  }
}