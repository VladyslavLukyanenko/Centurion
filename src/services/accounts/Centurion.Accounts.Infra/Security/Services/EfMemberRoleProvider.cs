using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.App.Security.Model;
using Centurion.Accounts.App.Security.Services;
using Centurion.Accounts.Core.Security;
using Centurion.Accounts.Infra.Services;

namespace Centurion.Accounts.Infra.Security.Services;

public class EfMemberRoleProvider : DataProvider, IMemberRoleProvider
{
  private readonly IQueryable<MemberRole> _memberRoles;
  private readonly IQueryable<UserMemberRoleBinding> _userMemberRoleBindings;
  private readonly IMapper _mapper;

  public EfMemberRoleProvider(DbContext context, IMapper mapper) : base(context)
  {
    _mapper = mapper;
    _memberRoles = GetAliveDataSource<MemberRole>();
    _userMemberRoleBindings = GetDataSource<UserMemberRoleBinding>();
  }

  public async ValueTask<IList<MemberRoleData>> GetMemberRolesAsync(Guid dashboardId, CancellationToken ct = default)
  {
    return await _memberRoles.Where(_ => _.DashboardId == dashboardId)
      .ProjectTo<MemberRoleData>(_mapper.ConfigurationProvider)
      .OrderBy(_ => _.Name)
      .ToListAsync(ct);
  }

  public async ValueTask<IList<BoundMemberRoleData>> GetRolesAsync(Guid dashboardId, long userId,
    CancellationToken ct = default)
  {
    var query = from role in _memberRoles
      join binding in _userMemberRoleBindings on role.Id equals binding.MemberRoleId
      where binding.DashboardId == dashboardId && binding.UserId == userId
      select new BoundMemberRoleData
      {
        Permissions = role.Permissions,
        RoleName = role.Name,
        MemberRoleId = role.Id,
        ColorHex = role.ColorHex,
        RoleBindingId = binding.Id
      };

    return await query.OrderBy(_ => _.RoleName).ToListAsync(ct);
  }
}