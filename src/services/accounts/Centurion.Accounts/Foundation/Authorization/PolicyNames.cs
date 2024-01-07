namespace Centurion.Accounts.Foundation.Authorization;

public abstract class SharedPolicyNames
{
  public const string AdministratorOnly = nameof(AdministratorOnly);
  public const string AdminsOrFeaturedOwnersOnly = nameof(AdminsOrFeaturedOwnersOnly);
  public const string AdminsOrOwnersOnly = nameof(AdminsOrOwnersOnly);
  public const string OwnerOnly = nameof(OwnerOnly);
}