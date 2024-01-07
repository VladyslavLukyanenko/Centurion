using CSharpFunctionalExtensions;

namespace Centurion.Accounts.Core.Security.Services;

public class MemberRoleManager : IMemberRoleManager
{
  private readonly IPermissionsRegistry _permissionsRegistry;
  private readonly IMemberRoleRepository _memberRoleRepository;

  public MemberRoleManager(IPermissionsRegistry permissionsRegistry, IMemberRoleRepository memberRoleRepository)
  {
    _permissionsRegistry = permissionsRegistry;
    _memberRoleRepository = memberRoleRepository;
  }

  public async ValueTask<Result<MemberRole>> CreateAsync(Guid dashboardId, string name,
    IEnumerable<string> permissions, decimal? salary, PayoutFrequency? payoutFrequency, Currency? currency,
    string? colorHex, CancellationToken ct = default)
  {
    var rolePermissions = permissions as string[] ?? permissions.ToArray();
    if (!rolePermissions.All(p =>
          _permissionsRegistry.SupportedPermissions.Contains(p, StringComparer.OrdinalIgnoreCase)))
    {
      return Result.Failure<MemberRole>("Invalid permissions provided.");
    }

    var role = new MemberRole(dashboardId, name, rolePermissions, salary, payoutFrequency, currency, colorHex);
    return await _memberRoleRepository.CreateAsync(role, ct);
  }
}