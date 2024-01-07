using Microsoft.AspNetCore.Authorization;

namespace Centurion.Accounts.Authorization;

public class AdminOrSameUserRequirement : IAuthorizationRequirement
{
  public AdminOrSameUserRequirement(long userId)
  {
    UserId = userId;
  }

  public long UserId { get; }
}