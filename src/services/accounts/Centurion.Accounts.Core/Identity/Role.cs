using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.Identity;

public class Role
  : Entity, IConcurrentEntity
{
  private Role()
  {
  }

  public Role(string roleName)
  {
    Name = roleName;
    NormalizedName = roleName.ToUpperInvariant();
  }

  /// <summary>Gets or sets the name for this role.</summary>
  public string Name { get; private set; } = null!;

  /// <summary>Gets or sets the normalized name for this role.</summary>
  public string NormalizedName { get; } = null!;

  /// <summary>
  ///   A random value that should change whenever a role is persisted to the store
  /// </summary>
  public string ConcurrencyStamp { get; } = null!;

  public void Rename(string name)
  {
    Name = name;
  }
}