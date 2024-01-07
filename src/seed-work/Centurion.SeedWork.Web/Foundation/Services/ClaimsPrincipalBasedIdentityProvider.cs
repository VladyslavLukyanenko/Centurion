using Microsoft.AspNetCore.Http;

namespace Centurion.SeedWork.Web.Foundation.Services;

public class ClaimsPrincipalBasedIdentityProvider
  : IIdentityProvider
{
  private readonly IHttpContextAccessor _httpContextAccessor;

  public ClaimsPrincipalBasedIdentityProvider(IHttpContextAccessor httpContextAccessor)
  {
    _httpContextAccessor = httpContextAccessor;
  }

  public string? GetCurrentIdentity()
  {
    return _httpContextAccessor.HttpContext?.User.GetUserId();
  }
}