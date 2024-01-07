using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.App.Security.Model;
using Centurion.Accounts.App.Security.Services;
using Centurion.Accounts.Authorization;
using Centurion.Accounts.Core.Collections;
using Centurion.Accounts.Core.Security.Services;
using Centurion.Accounts.Foundation.Authorization;
using Centurion.Accounts.Foundation.Model;
using Centurion.Accounts.Foundation.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Centurion.Accounts.Security.Controllers;

public class MemberRoleBindingsController : SecuredDashboardBoundControllerBase
{
  private readonly IMemberRoleBindingProvider _memberRoleBindingProvider;
  private readonly IUserMemberRoleBindingRepository _userMemberRoleBindingRepository;
  private readonly IMemberRoleBindingService _memberRoleBindingService;

  public MemberRoleBindingsController(IServiceProvider provider, IMemberRoleBindingProvider memberRoleBindingProvider,
    IUserMemberRoleBindingRepository userMemberRoleBindingRepository,
    IMemberRoleBindingService memberRoleBindingService)
    : base(provider)
  {
    _memberRoleBindingProvider = memberRoleBindingProvider;
    _userMemberRoleBindingRepository = userMemberRoleBindingRepository;
    _memberRoleBindingService = memberRoleBindingService;
  }

  [HttpGet]
  [AuthorizePermission(Permissions.StaffManage)]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<IPagedList<StaffMemberData>>))]
  public async ValueTask<IActionResult> GetStaffMembersPageAsync([FromQuery] StaffMemberPageRequest pageRequest,
    CancellationToken ct)
  {
    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(CurrentDashboardId)
      .OrThrowForbid();

    var page = await _memberRoleBindingProvider.GetMembersPageAsync(CurrentDashboardId, pageRequest, ct);
    return Ok(page);
  }

  [HttpGet("Roles")]
  [AuthorizePermission(Permissions.StaffManage)]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<IList<StaffRoleMembersData>>))]
  public async ValueTask<IActionResult> GetRolesAsync(CancellationToken ct)
  {
    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(CurrentDashboardId)
      .OrThrowForbid();

    var list = await _memberRoleBindingProvider.GetRolesAsync(CurrentDashboardId, ct);
    return Ok(list);
  }

  [HttpGet("{userId:long}/Summary")]
  [AuthorizePermission(Permissions.StaffManage)]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<MemberSummaryData>))]
  public async ValueTask<IActionResult> GetSummaryAsync(long userId, CancellationToken ct)
  {
    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(CurrentDashboardId)
      .OrThrowForbid();

    var summary = await _memberRoleBindingProvider.GetSummaryAsync(userId, CurrentDashboardId, ct);
    if (summary == null)
    {
      return NotFound();
    }

    return Ok(summary);
  }

  [HttpPost]
  [AuthorizePermission(Permissions.StaffManage)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async ValueTask<IActionResult> AssignRolesAsync([FromBody] IList<MemberRoleAssignmentData> assignments,
    CancellationToken ct)
  {
    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(CurrentDashboardId)
      .OrThrowForbid();

    var r = await _memberRoleBindingService.AssignRolesAsync(CurrentDashboardId, assignments, ct);
    if (r.IsFailure)
    {
      return BadRequest(r.Error);
    }

    return NoContent();
  }


  [HttpDelete("{userId:long}/Roles/{roleId:long}")]
  [AuthorizePermission(Permissions.StaffDelete)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async ValueTask<IActionResult> RemoveMemberAsync(long userId, long roleId, CancellationToken ct)
  {
    var binding = await _userMemberRoleBindingRepository.GetUserRoleBindingAsync(userId, roleId, ct);
    if (binding == null)
    {
      return NotFound();
    }

    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(binding.DashboardId)
      .OrThrowForbid();

    _userMemberRoleBindingRepository.Remove(binding);
    return NoContent();
  }
}