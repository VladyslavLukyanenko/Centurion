namespace Centurion.Accounts.Core.Security.Services;

public interface IPermissionsAccessor
{
  IReadOnlyList<string> CurrentRequiredPermissions { get; }
}