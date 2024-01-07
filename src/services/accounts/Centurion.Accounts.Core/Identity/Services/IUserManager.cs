namespace Centurion.Accounts.Core.Identity.Services;

public interface IUserManager
{
  Task<string?> CreateAsync(User user, string password, CancellationToken ct = default);
  Task<string?> AddToRolesAsync(User user, IEnumerable<string> roleNames, CancellationToken ct = default);
  Task<string?> DeleteAsync(User user, CancellationToken ct = default);

  Task<string?> ChangePasswordAsync(User user, string currentPassword, string newPassword,
    CancellationToken ct = default);

  Task<string?> CreateAsync(User user, CancellationToken ct = default);
}