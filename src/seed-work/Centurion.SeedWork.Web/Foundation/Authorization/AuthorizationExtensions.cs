using System.Security.Claims;

namespace Centurion.SeedWork.Web.Foundation.Authorization;

public static class AuthorizationExtensions
{
  public static string? GetUserId(this ClaimsPrincipal self)
  {
    return self.FindFirst("id")?.Value;
  }
}