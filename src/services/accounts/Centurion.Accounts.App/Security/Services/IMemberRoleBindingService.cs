using Centurion.Accounts.App.Security.Model;
using CSharpFunctionalExtensions;

namespace Centurion.Accounts.App.Security.Services;

public interface IMemberRoleBindingService
{
  ValueTask<Result> AssignRolesAsync(Guid dashboardId, IEnumerable<MemberRoleAssignmentData> data,
    CancellationToken ct = default);
}