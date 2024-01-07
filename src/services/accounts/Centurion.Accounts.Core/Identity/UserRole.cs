namespace Centurion.Accounts.Core.Identity;

public class UserRole
{
  private Role _role = null!;

  // for ef
  private User _user = null!;

  private UserRole()
  {
  }

  public UserRole(User user, Role role)
  {
    _user = user;
    _role = role;
    UserId = user.Id;
    RoleId = role.Id;
  }

  public long UserId { get; }
  public long RoleId { get; }
}