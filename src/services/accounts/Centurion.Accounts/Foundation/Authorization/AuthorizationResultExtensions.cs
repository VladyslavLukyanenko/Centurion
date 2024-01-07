using Microsoft.AspNetCore.Authorization;

namespace Centurion.Accounts.Foundation.Authorization;

public static class AuthorizationResultExtensions
{
  public static async ValueTask OrThrowForbid(this ValueTask<AuthorizationResult> pendingResult)
  {
    var result = await pendingResult;
    if (!result.Succeeded)
    {
      throw new AuthorizationException();
    }
  }

  public static async ValueTask<AuthorizationResult> Or(this ValueTask<AuthorizationResult> pendingResult,
    Func<ValueTask<AuthorizationResult>> otherwiseProvider)
  {
    var result = await pendingResult;
    if (result.Succeeded)
    {
      return result;
    }

    var otherwise = await otherwiseProvider();
    if (otherwise.Succeeded)
    {
      return otherwise;
    }

    throw new AuthorizationException();
  }
}