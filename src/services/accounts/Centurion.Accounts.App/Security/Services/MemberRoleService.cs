using Centurion.Accounts.App.Security.Model;
using CSharpFunctionalExtensions;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Core.Security;
using Centurion.Accounts.Core.Security.Services;

namespace Centurion.Accounts.App.Security.Services;

public class MemberRoleService : IMemberRoleService
{
  private readonly IMemberRoleManager _memberRoleManager;
  private readonly IMemberRoleRepository _memberRoleRepository;

  public MemberRoleService(IMemberRoleManager memberRoleManager, IMemberRoleRepository memberRoleRepository)
  {
    _memberRoleManager = memberRoleManager;
    _memberRoleRepository = memberRoleRepository;
  }

  public async ValueTask<Result<MemberRole>> CreateAsync(Guid dashboardId, MemberRoleData data,
    CancellationToken ct = default)
  {
    return await _memberRoleManager.CreateAsync(dashboardId, data.Name, data.Permissions, data.Salary,
      data.PayoutFrequency.ToEnumeration<PayoutFrequency?>(), data.Currency.ToEnumeration<Currency?>(), data.ColorHex,
      ct);
  }

  public ValueTask UpdateAsync(MemberRole role, MemberRoleData data, CancellationToken ct = default)
  {
    role.ChangePermissions(data.Permissions);
    _memberRoleRepository.Update(role);
    return default;
  }
}