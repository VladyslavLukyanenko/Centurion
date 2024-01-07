namespace Centurion.Accounts.Core.Identity.Services;

public class UserService : IUserService
{
  private readonly IUserManager _userManager;
  private readonly IUserRepository _userRepository;

  public UserService(IUserManager userManager, IUserRepository userRepository)
  {
    _userManager = userManager;
    _userRepository = userRepository;
  }
  //
  // public async Task<User> CreateConfirmed(string email, string password, IList<string> roleNames,
  //   string name, CancellationToken ct = default)
  // {
  //   var user = User.CreateConfirmed(email, name);
  //   var creationResult = await _userManager.CreateAsync(user, password, ct);
  //   if (!string.IsNullOrEmpty(creationResult))
  //   {
  //     throw new CoreException("Can't create user: " + creationResult);
  //   }
  //
  //   var addRolesResult = await _userManager.AddToRolesAsync(user, roleNames, ct);
  //   if (!string.IsNullOrEmpty(addRolesResult))
  //   {
  //     throw new CoreException("Can't add roles to user: " + addRolesResult);
  //   }
  //
  //   return user;
  // }
  //
  // public async Task<User> RegisterAsync(Email email, string password, string name,
  //   CancellationToken ct = default)
  // {
  //   var user = User.CreateWithEmail(email, name);
  //   var creationResult = await _userManager.CreateAsync(user, password, ct);
  //   if (!string.IsNullOrEmpty(creationResult))
  //   {
  //     throw new CoreException("Can't create user: " + creationResult);
  //   }
  //
  //   return user;
  // }

  public async Task<bool> RemoveAsync(long userId, CancellationToken ct = default)
  {
    var user = await _userRepository.GetByIdAsync(userId, ct);
    if (user is null)
    {
      return false;
    }

    var result = await _userManager.DeleteAsync(user, ct);
    if (!string.IsNullOrEmpty(result))
    {
      throw new CoreException("Can't remove user: " + result);
    }

    return true;
  }

  public async Task ChangePasswordAsync(User user, string currentPassword, string newPassword,
    CancellationToken ct = default)
  {
    var creationResult = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword, ct);
    if (!string.IsNullOrEmpty(creationResult))
    {
      throw new CoreException("Can't change password: " + creationResult);
    }
  }
}