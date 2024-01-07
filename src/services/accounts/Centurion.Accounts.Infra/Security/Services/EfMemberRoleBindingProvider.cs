using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.App.Security.Services;
using Centurion.Accounts.Core.Collections;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Security;
using Centurion.Accounts.Infra.Services;

namespace Centurion.Accounts.Infra.Security.Services;

public class EfMemberRoleBindingProvider : DataProvider, IMemberRoleBindingProvider
{
  private readonly IQueryable<User> _users;
  private readonly IQueryable<Member> _members;
  private readonly IMemberRoleProvider _memberRoleProvider;
  private readonly IQueryable<MemberRole> _memberRoles;
  private readonly IQueryable<UserMemberRoleBinding> _userMemberRoleBindings;

  public EfMemberRoleBindingProvider(DbContext context, IMemberRoleProvider memberRoleProvider)
    : base(context)
  {
    _memberRoleProvider = memberRoleProvider;
    _users = GetDataSource<User>();
    _members = GetDataSource<Member>();
    _memberRoles = GetAliveDataSource<MemberRole>();
    _userMemberRoleBindings = GetDataSource<UserMemberRoleBinding>();
  }

  public async ValueTask<IPagedList<StaffMemberData>> GetMembersPageAsync(Guid dashboardId,
    StaffMemberPageRequest pageRequest, CancellationToken ct)
  {
    var query = from member in _members
      join user in _users on member.UserId equals user.Id
      where member.DashboardId == dashboardId
      select new
      {
        user.Name,
        user.Discriminator,
        user.Id,
        Email = user.Email.NormalizedValue,
        user.Avatar,
      };

    if (!pageRequest.IsSearchTermEmpty())
    {
      var normalizedSearchTerm = pageRequest.NormalizeSearchTerm();

      query = query.Where(_ =>
        _.Email.Contains(normalizedSearchTerm)
        || _.Discriminator.Contains(normalizedSearchTerm)
        || _.Name.Contains(normalizedSearchTerm));
    }

    if (!pageRequest.IncludeStaff)
    {
      query = query.Where(_ =>
        _userMemberRoleBindings.All(b => _.Id != b.UserId && b.DashboardId == dashboardId));
    }

    var page = await query.PaginateAsync(pageRequest, ct);
    return page.ProjectTo(user => new StaffMemberData
    {
      Name = user.Name,
      Discriminator = user.Discriminator,
      Id = user.Id,
      Avatar = user.Avatar
    });
  }

  public async ValueTask<MemberSummaryData?> GetSummaryAsync(long userId, Guid dashboardId,
    CancellationToken ct = default)
  {
    var query = from m in _members
      join u in _users on m.UserId equals u.Id
      where m.DashboardId == dashboardId && m.UserId == userId
      select new MemberSummaryData
      {
        UserId = m.UserId,
        DashboardId = m.DashboardId,
        JoinedAt = m.JoinedAt,
        Avatar = u.Avatar,
        Discriminator = u.Discriminator,
        DiscordId = u.DiscordId,
        Name = u.Name,
        DiscordRoles = u.DiscordRoles
      };

    var member = await query.FirstOrDefaultAsync(ct);
    if (member == null)
    {
      return null;
    }

    member.Roles = await _memberRoleProvider.GetRolesAsync(dashboardId, userId, ct);

    return member;
  }

  public async ValueTask<IList<StaffRoleMembersData>> GetRolesAsync(Guid dashboardId, CancellationToken ct = default)
  {
    var query = from role in _memberRoles
      join binding in _userMemberRoleBindings on role.Id equals binding.MemberRoleId
        into tmp
      from binding in tmp.DefaultIfEmpty()
      join user in _users on binding.UserId equals user.Id
        into utmp
      from user in utmp.DefaultIfEmpty()
      where role.DashboardId == dashboardId
      select new RawMemberRole
      {
        UserName = user.Name,
        Discriminator = user.Discriminator,
        DiscordId = user.DiscordId,
        UserId = user.Id,
        UserAvatar = user.Avatar,
        RoleName = role.Name,
        ColorHex = role.ColorHex,
        RoleId = role.Id
      };

    var items = (await query.ToArrayAsync(ct))
      .ToLookup(_ => _.RoleId);

    var roles = new List<StaffRoleMembersData>(items.Count);
    foreach (var group in items)
    {
      var roleId = group.Key;
      var role = new StaffRoleMembersData
      {
        Id = roleId
      };

      roles.Add(role);

      foreach (var member in group)
      {
        role.Name = member.RoleName;
        if (!member.UserId.HasValue)
        {
          break;
        }

        var memberData = new StaffMemberData
        {
          Avatar = member.UserAvatar,
          Discriminator = member.Discriminator!,
          Id = member.UserId.Value,
          Name = member.UserName!
        };

        role.Members.Add(memberData);
      }
    }

    return roles.OrderBy(_ => _.Name).ToList();
  }

  private class RawMemberRole
  {
    public string? UserName { get; set; }
    public string? Discriminator { get; set; }
    public ulong? DiscordId { get; set; }
    public long? UserId { get; set; }
    public string? UserAvatar { get; set; }
    public string RoleName { get; set; } = null!;
    public string? ColorHex { get; set; }
    public long RoleId { get; set; }
  }
}