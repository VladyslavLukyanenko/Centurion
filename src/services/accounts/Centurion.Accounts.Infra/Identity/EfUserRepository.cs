using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Identity.Services;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Infra.Repositories;

namespace Centurion.Accounts.Infra.Identity;

public class EfUserRepository
  : EfCrudRepository<User, long>, IUserRepository, IUserEmailStore<User>, IUserPasswordStore<User>,
    IUserClaimStore<User>, IUserSecurityStampStore<User>, IUserLockoutStore<User>,
    IUserRoleStore<User>, IRoleStore<Role>
{
  public EfUserRepository(DbContext context, IUnitOfWork unitOfWork)
    : base(context, unitOfWork)
  {
  }

  private IQueryable<Role> Roles => Context.Set<Role>();
  private IQueryable<UserRole> UserRoles => Context.Set<UserRole>();
  private IQueryable<UserClaim> UserClaims => Context.Set<UserClaim>();

  public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
  {
    await Context.AddAsync(role, cancellationToken);
    await Context.SaveChangesAsync(cancellationToken);
    return IdentityResult.Success;
  }

  public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if ((object) role == null)
    {
      throw new ArgumentNullException(nameof(role));
    }

    Context.Attach(role);
    Context.Update(role);
    try
    {
      await Context.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateConcurrencyException ex)
    {
      return IdentityResult.Failed(new IdentityError
      {
        Code = "ConcurrencyFailure",
        Description = ex.Message
      });
    }

    return IdentityResult.Success;
  }

  public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();
    if ((object) role == null)
    {
      throw new ArgumentNullException(nameof(role));
    }

    Context.Remove(role);
    try
    {
      await Context.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateConcurrencyException ex)
    {
      return IdentityResult.Failed(new IdentityError
      {
        Code = "ConcurrencyFailure",
        Description = ex.Message
      });
    }

    return IdentityResult.Success;
  }

  public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
  {
    return Task.FromResult(role.Id.ToString());
  }

  public Task<string> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
  {
    return Task.FromResult(role.Name);
  }

  public Task SetRoleNameAsync(Role role, string roleName, CancellationToken cancellationToken)
  {
    role.Rename(roleName);
    return Task.CompletedTask;
  }

  public Task<string> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
  {
    return Task.FromResult(role.NormalizedName);
  }

  public Task SetNormalizedRoleNameAsync(Role role, string normalizedName, CancellationToken cancellationToken)
  {
//            role.Rename(roleName);
    return Task.CompletedTask;
  }

  async Task<Role> IRoleStore<Role>.FindByIdAsync(string roleId, CancellationToken cancellationToken)
  {
    var id = long.Parse(roleId);
    return await Roles.FirstOrDefaultAsync(_ => _.Id == id, cancellationToken);
  }

  Task<Role> IRoleStore<Role>.FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
  {
    return Roles.FirstOrDefaultAsync(_ => _.NormalizedName == normalizedRoleName, cancellationToken);
  }

  public async Task<IList<Claim>> GetClaimsAsync(User user, CancellationToken cancellationToken)
  {
    var claims = await UserClaims.Where(_ => _.UserId == user.Id)
      .ToListAsync(cancellationToken);

    return claims.Select(claim => claim.ToClaim())
      .ToList();
  }

  public async Task AddClaimsAsync(User user, IEnumerable<Claim> claims,
    CancellationToken cancellationToken)
  {
    await Context.AddRangeAsync(claims.Select(claim =>
    {
      var ic = new UserClaim(user.Id);
      ic.InitializeFromClaim(claim);

      return ic;
    }), cancellationToken);

    await Context.SaveChangesAsync(cancellationToken);
  }

  public async Task ReplaceClaimAsync(User user, Claim claim, Claim newClaim,
    CancellationToken cancellationToken)
  {
    var claimToRemove = await UserClaims.FirstOrDefaultAsync(_ =>
      _.UserId == user.Id && _.ClaimType == claim.Type && _.ClaimValue == claim.Value, cancellationToken);
    Context.Remove(claimToRemove);

    var newIc = new UserClaim(user.Id);

    newIc.InitializeFromClaim(newClaim);
    await Context.AddAsync(newIc, cancellationToken);

    await Context.SaveChangesAsync(cancellationToken);
  }

  public async Task RemoveClaimsAsync(User user, IEnumerable<Claim> claims,
    CancellationToken cancellationToken)
  {
    Context.RemoveRange(claims.Select(claim =>
    {
      var ic = new UserClaim(user.Id);

      ic.InitializeFromClaim(claim);

      return ic;
    }));

    await Context.SaveChangesAsync(cancellationToken);
  }

  public async Task<IList<User>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
  {
    return await DataSource
      .Where(user => UserClaims.Where(_ => _.ClaimType == claim.Type && _.ClaimValue == claim.Value)
        .Select(_ => _.UserId)
        .Contains(user.Id))
      .ToListAsync(cancellationToken);
  }

  public void Dispose()
  {
  }

  public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
  {
    return Task.FromResult(user.Id.ToString());
  }

  public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
  {
    return Task.FromResult(user.UserName);
  }

  public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
  {
    user.UserName = userName;

    return Task.CompletedTask;
  }

  public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
  {
    return Task.FromResult(user.NormalizedUserName);
  }

  public Task SetNormalizedUserNameAsync(User user, string normalizedName,
    CancellationToken cancellationToken)
  {
    user.NormalizedUserName = normalizedName;

    return Task.CompletedTask;
  }

  async Task<IdentityResult> IUserStore<User>.CreateAsync(User user, CancellationToken cancellationToken)
  {
    await CreateAsync(user, cancellationToken);
    await Context.SaveChangesAsync(cancellationToken);

    return IdentityResult.Success;
  }

  public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
  {
    Update(user);

    try
    {
      await Context.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateConcurrencyException)
    {
      return IdentityResult.Failed(new IdentityError
      {
        Code = "OptimisticConcurrencyFailure",
        Description = "Optimistic concurrency failure, object has been modified."
      });
    }

    return IdentityResult.Success;
  }

  public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
  {
    Remove(user);

    try
    {
      await Context.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateConcurrencyException)
    {
      return IdentityResult.Failed(new IdentityError
      {
        Code = "OptimisticConcurrencyFailure",
        Description = "Optimistic concurrency failure, object has been modified."
      });
    }

    return IdentityResult.Success;
  }

  public Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
  {
    var id = long.Parse(userId);
    return FindWithCacheAsync(u => u.Id == id, cancellationToken);
  }

  public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
  {
    return FindWithCacheAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);
  }

  public Task SetEmailAsync(User user, string email, CancellationToken cancellationToken)
  {
    throw new NotSupportedException("User's email can't be changed");
  }

  public Task<string> GetEmailAsync(User user, CancellationToken cancellationToken)
  {
    return Task.FromResult(user.Email.Value);
  }

  public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
  {
    return Task.FromResult(user.Email.IsConfirmed);
  }

  public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
  {
    user.SetEmail(user.Email.Value, confirmed);

    return Task.CompletedTask;
  }

  public async Task<User> FindByEmailAsync(string email, CancellationToken cancellationToken)
  {
    string normalizedEmail = email.ToUpperInvariant();
    return await FindWithCacheAsync(_ => _.Email.NormalizedValue == normalizedEmail, cancellationToken);
  }

  public Task<string> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
  {
    return Task.FromResult(user.Email.NormalizedValue);
  }

  public Task SetNormalizedEmailAsync(User user, string normalizedEmail,
    CancellationToken cancellationToken)
  {
    // user.NormalizedEmail = normalizedEmail;

    return Task.CompletedTask;
  }

  public Task<DateTimeOffset?> GetLockoutEndDateAsync(User user, CancellationToken cancellationToken)
  {
    return Task.FromResult(user.LockoutEnd);
  }

  public async Task SetLockoutEndDateAsync(User user, DateTimeOffset? lockoutEnd,
    CancellationToken cancellationToken)
  {
    user.LockoutEnd = lockoutEnd;
    await Context.SaveChangesAsync(cancellationToken);
  }

  public async Task<int> IncrementAccessFailedCountAsync(User user, CancellationToken cancellationToken)
  {
    ++user.AccessFailedCount;
    await Context.SaveChangesAsync(cancellationToken);

    return user.AccessFailedCount;
  }

  public async Task ResetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
  {
    user.AccessFailedCount = 0;
    await Context.SaveChangesAsync(cancellationToken);
  }

  public Task<int> GetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
  {
    return Task.FromResult(user.AccessFailedCount);
  }

  public Task<bool> GetLockoutEnabledAsync(User user, CancellationToken cancellationToken)
  {
    return Task.FromResult(user.LockoutEnabled);
  }

  public async Task SetLockoutEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
  {
    user.LockoutEnabled = enabled;
    await Context.SaveChangesAsync(cancellationToken);
  }

  public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
  {
    user.PasswordHash = passwordHash;
    return Task.CompletedTask;
  }

  public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
  {
    return Task.FromResult(user.PasswordHash);
  }

  public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
  {
    return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
  }

  public async Task<IList<string>> GetRolesAsync(long userId, CancellationToken ct = default)
  {
    return await Roles
      .Where(role => UserRoles.Any(_ => _.RoleId == role.Id && _.UserId == userId))
      .Select(_ => _.Name)
      .ToListAsync(ct);
  }

  public Task<User> GetByEmailAsync(string subject, CancellationToken ct = default)
  {
    var normalizedSubj = subject.ToUpper();
    return DataSource.FirstOrDefaultAsync(_ => _.Email.NormalizedValue == normalizedSubj, ct)!;
  }

  public Task<User> GetByDiscordIdAsync(ulong discordId, CancellationToken ct = default)
  {
    return DataSource.FirstOrDefaultAsync(_ => _.DiscordId == discordId, ct);
  }

  public async Task AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
  {
    Role role = await Roles.FirstOrDefaultAsync(_ => _.NormalizedName == roleName.ToUpper(),
      cancellationToken);

    if (role == null)
    {
      role = new Role(roleName);

      await Context.AddAsync(role, cancellationToken);
    }

    await Context.AddAsync(new UserRole(user, role), cancellationToken);
    await Context.SaveChangesAsync(cancellationToken);
  }

  public async Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken)
  {
    var mapping = await UserRoles.FirstOrDefaultAsync(
      _ => _.UserId == user.Id && Roles.Any(role => role.NormalizedName == roleName.ToUpper()),
      cancellationToken);

    if (mapping == null)
    {
      return;
    }

    Context.Remove(mapping);

    await Context.SaveChangesAsync(cancellationToken);
  }

  public Task<IList<string>> GetRolesAsync(User user, CancellationToken ct)
  {
    return GetRolesAsync(user.Id, ct);
  }

  public async Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken)
  {
    return await UserRoles.AnyAsync(
      _ => _.UserId == user.Id && Roles.Any(role => role.NormalizedName == roleName.ToUpper()),
      cancellationToken);
  }

  public async Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
  {
    return await DataSource.Where(u =>
        UserRoles.Any(m => Roles.Any(
          r => r.NormalizedName == roleName.ToUpper())))
      .ToListAsync(cancellationToken);
  }

  public async Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken)
  {
    user.SecurityStamp = stamp;
    await Context.SaveChangesAsync(cancellationToken);
  }

  public Task<string> GetSecurityStampAsync(User user, CancellationToken cancellationToken)
  {
    return Task.FromResult(user.SecurityStamp);
  }

  private Task<User> FindWithCacheAsync(Expression<Func<User, bool>> predicate,
    CancellationToken cancellationToken)
  {
    return DataSource.FirstOrDefaultAsync(predicate, cancellationToken);
  }

  public async Task<IList<long>> GetUserIdsInRoleAsync(string roleName, CancellationToken ct = default)
  {
    var normalizedRoleName = roleName.ToUpperInvariant();
    var query = from user in DataSource
      join userRole in UserRoles on user.Id equals userRole.UserId
      join role in Roles on userRole.RoleId equals role.Id
      where role.NormalizedName == normalizedRoleName
      select user.Id;

    return await query.Distinct().ToArrayAsync(ct);
  }
}