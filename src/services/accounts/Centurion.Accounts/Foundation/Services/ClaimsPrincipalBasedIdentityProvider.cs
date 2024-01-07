using Centurion.Accounts.Core.Identity.Services;
using Centurion.Accounts.Foundation.Authorization;

namespace Centurion.Accounts.Foundation.Services;

public class ClaimsPrincipalBasedIdentityProvider : IIdentityProvider
{
  private readonly IHttpContextAccessor _httpContextAccessor;

  public ClaimsPrincipalBasedIdentityProvider(IHttpContextAccessor httpContextAccessor)
  {
    _httpContextAccessor = httpContextAccessor;
  }

  public long? GetCurrentIdentity()
  {
    return _httpContextAccessor.HttpContext?.User.GetUserId();
  }
}