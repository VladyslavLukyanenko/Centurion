using Centurion.Accounts.App.Security.Model;
using Centurion.Accounts.App.Security.Services;
using Centurion.Accounts.Authorization;
using Centurion.Accounts.Core.Security;
using Centurion.Accounts.Core.Security.Services;
using Centurion.Accounts.Foundation.Authorization;
using Centurion.Accounts.Foundation.Model;
using Centurion.Accounts.Foundation.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Centurion.Accounts.Security.Controllers;

public class MemberRolesController : SecuredDashboardBoundControllerBase
{
  private readonly IMemberRoleService _memberRoleService;
  private readonly IMemberRoleRepository _memberRoleRepository;
  private readonly IMemberRoleProvider _memberRoleProvider;

  public MemberRolesController(IServiceProvider provider, IMemberRoleService memberRoleService,
    IMemberRoleRepository memberRoleRepository, IMemberRoleProvider memberRoleProvider)
    : base(provider)
  {
    _memberRoleService = memberRoleService;
    _memberRoleRepository = memberRoleRepository;
    _memberRoleProvider = memberRoleProvider;
  }

  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<MemberRoleData[]>))]
  [AuthorizePermission(Permissions.RolesManage)]
  public async ValueTask<IActionResult> GetRolesAsync(CancellationToken ct)
  {
    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(CurrentDashboardId)
      .OrThrowForbid();

    var roles = await _memberRoleProvider.GetMemberRolesAsync(CurrentDashboardId, ct);
    return Ok(roles);
  }

  [HttpPost]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [AuthorizePermission(Permissions.RolesManage)]
  public async ValueTask<IActionResult> CreateAsync([FromBody] MemberRoleData role, CancellationToken ct)
  {
    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(CurrentDashboardId)
      .OrThrowForbid();

    await _memberRoleService.CreateAsync(CurrentDashboardId, role, ct);
    return NoContent();
  }

  [HttpPost("{roleId:long}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [AuthorizePermission(Permissions.RolesManage)]
  public async ValueTask<IActionResult> UpdateAsync(long roleId, [FromBody] MemberRoleData data, CancellationToken ct)
  {
    MemberRole? role = await _memberRoleRepository.GetByIdAsync(roleId, ct);
    if (role == null)
    {
      return NotFound();
    }

    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(role.DashboardId)
      .OrThrowForbid();

    await _memberRoleService.UpdateAsync(role, data, ct);
    return NoContent();
  }

  [HttpDelete("{roleId:long}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [AuthorizePermission(Permissions.RolesDelete)]
  public async ValueTask<IActionResult> RemoveRoleAsync(long roleId, CancellationToken ct)
  {
    MemberRole? role = await _memberRoleRepository.GetByIdAsync(roleId, ct);
    if (role == null)
    {
      return NotFound();
    }

    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(role.DashboardId)
      .OrThrowForbid();

    _memberRoleRepository.Remove(role);
    return NoContent();
  }
}