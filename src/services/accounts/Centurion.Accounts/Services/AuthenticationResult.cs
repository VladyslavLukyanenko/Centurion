using System.Security.Claims;
using Centurion.Accounts.Core.Identity;

namespace Centurion.Accounts.Services;

public class AuthenticationResult
{
  public IList<Claim> Claims { get; init; } = null!;
  public User AuthenticatedUser { get; init; } = null!;
}