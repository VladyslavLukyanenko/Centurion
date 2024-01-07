using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Infra.Services;

namespace Centurion.Accounts.Infra.Products.Services;

public class EfDashboardProvider : DataProvider, IDashboardProvider
{
  private readonly IQueryable<Dashboard> _aliveDashboards;
  private readonly IMapper _mapper;

  public EfDashboardProvider(DbContext context, IMapper mapper)
    : base(context)
  {
    _mapper = mapper;
    _aliveDashboards = GetAliveDataSource<Dashboard>();
  }

  public async ValueTask<DashboardData?> GetByOwnerIdAsync(long ownerId, CancellationToken ct = default)
  {
    var dash = await _aliveDashboards.Where(_ => _.OwnerId == ownerId)
      .SingleOrDefaultAsync(ct);

    return _mapper.Map<DashboardData?>(dash);
  }

  public async ValueTask<DashboardLoginData?> GetLoginDataAsync(
    IEnumerable<KeyValuePair<DashboardHostingMode, string>> modes, CancellationToken ct = default)
  {
    var normalizedModes = modes.Select(m => $"{(int) m.Key}__{m.Value.ToLower()}");
    return await _aliveDashboards.Where(
        d => normalizedModes.Contains(d.HostingConfig.Mode + "__" + d.HostingConfig.DomainName.ToLower()))
      .Select(d => new DashboardLoginData
      {
        DashboardId = d.Id,
        DiscordAuthorizeUrl = d.DiscordConfig.OAuthConfig.BuildAuthorizeUrl()
      })
      .FirstOrDefaultAsync(ct);
  }

  public async ValueTask<ProductPublicInfoData?> GetPublicByDashboardIdAsync(Guid dashboardId,
    CancellationToken ct = default)
  {
    var info = await _aliveDashboards.Where(p => p.Id == dashboardId)
      .Select(_ => _.ProductInfo)
      .SingleOrDefaultAsync(ct);

    return _mapper.Map<ProductPublicInfoData?>(info);
  }
}