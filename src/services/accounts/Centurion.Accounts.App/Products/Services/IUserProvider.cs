using Centurion.Accounts.App.Identity.Model;

namespace Centurion.Accounts.App.Products.Services;

public interface IUserProvider
{
  Task<UserData?> GetUserByIdAsync(long id, CancellationToken ct = default);
}