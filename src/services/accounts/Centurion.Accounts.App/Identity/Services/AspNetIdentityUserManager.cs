using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Identity.Services;

namespace Centurion.Accounts.App.Identity.Services;

public class AspNetIdentityUserManager : UserManager<User>, IUserManager
{
  public AspNetIdentityUserManager(IUserStore<User> store, IOptions<IdentityOptions> optionsAccessor,
    IPasswordHasher<User> passwordHasher, IEnumerable<IUserValidator<User>> userValidators,
    IEnumerable<IPasswordValidator<User>> passwordValidators, ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<User>> logger) : base(store,
    optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
  {
  }

  public async Task<string?> CreateAsync(User user, string password, CancellationToken ct = default)
  {
    ct.ThrowIfCancellationRequested();

    var result = await base.CreateAsync(user, password);
    if (!result.Succeeded)
    {
      return IdentityResultToString(result);
    }

    return null;
  }

  public async Task<string?> AddToRolesAsync(User user, IEnumerable<string> roleNames, CancellationToken ct = default)
  {
    ct.ThrowIfCancellationRequested();
    var result = await base.AddToRolesAsync(user, roleNames);
    if (!result.Succeeded)
    {
      return IdentityResultToString(result);
    }

    return null;
  }

  public async Task<string?> DeleteAsync(User user, CancellationToken ct = default)
  {
    ct.ThrowIfCancellationRequested();
    var result = await base.DeleteAsync(user);
    if (!result.Succeeded)
    {
      return IdentityResultToString(result);
    }

    return null;
  }

  public async Task<string?> ChangePasswordAsync(User user, string currentPassword, string newPassword,
    CancellationToken ct = default)
  {
    ct.ThrowIfCancellationRequested();
    var result = await base.ChangePasswordAsync(user, currentPassword, newPassword);
    if (!result.Succeeded)
    {
      return IdentityResultToString(result);
    }

    return null;
  }

  public async Task<string?> CreateAsync(User user, CancellationToken ct = default)
  {
    ct.ThrowIfCancellationRequested();

    var result = await base.CreateAsync(user);
    if (!result.Succeeded)
    {
      return IdentityResultToString(result);
    }

    return null;
  }

  private static string IdentityResultToString(IdentityResult creationResult)
  {
    return string.Join(",", creationResult.Errors.Select(_ => _.Description));
  }
}