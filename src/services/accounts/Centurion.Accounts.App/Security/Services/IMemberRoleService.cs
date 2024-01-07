using Centurion.Accounts.App.Security.Model;
using CSharpFunctionalExtensions;
using Centurion.Accounts.Core.Security;

namespace Centurion.Accounts.App.Security.Services;

public interface IMemberRoleService
{
  ValueTask<Result<MemberRole>> CreateAsync(Guid dashboardId, MemberRoleData data, CancellationToken ct = default);
  ValueTask UpdateAsync(MemberRole role, MemberRoleData data, CancellationToken ct = default);
}