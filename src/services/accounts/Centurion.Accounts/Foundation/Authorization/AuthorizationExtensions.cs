using System.Security.Claims;
using System.Security.Principal;

namespace Centurion.Accounts.Foundation.Authorization;

public static class AuthorizationExtensions
{
  public static bool HasAdminRole(this IPrincipal self) => self.IsInRole(SupportedRoleNames.Administrator);

  public static bool OwnsDashboard(this ClaimsPrincipal self, Guid dashboardId) =>
    self.GetOwnDashboardIds().Contains(dashboardId);

  public static bool JoinedDashboard(this ClaimsPrincipal self, Guid dashboardId) =>
    self.GetJoinedDashboardIds().Contains(dashboardId);

  public static long GetUserId(this ClaimsPrincipal self)
  {
    if (long.TryParse(self.FindFirst(AppClaimNames.Id)?.Value ?? "", out var userId))
    {
      return userId;
    }

    return 0L;
  }

  public static Guid? GetDashboardId(this ClaimsPrincipal self)
  {
    if (Guid.TryParse(self.FindFirst(AppClaimNames.CurrentDashboardId)?.Value ?? "", out var dashboardId))
    {
      return dashboardId;
    }

    return null;
  }

  public static string? GetStripeCustomerId(this ClaimsPrincipal self) =>
    self.FindFirstValue(AppClaimNames.StripeCustomerId);

  public static string? GetLicenseKey(this ClaimsPrincipal self) => self.FindFirstValue(AppClaimNames.LicenseKey);


  public static string? GetDiscordAccessToken(this ClaimsPrincipal self) =>
    self.FindFirstValue(AppClaimNames.DiscordAccessTokenToken);

  public static IEnumerable<string> GetPermissions(this ClaimsPrincipal self) =>
    self.Claims.Where(_ => _.Type == AppClaimNames.Permission)
      .Select(c => c.Value);

  public static IEnumerable<Guid> GetOwnDashboardIds(this ClaimsPrincipal self) =>
    self.Claims.Where(_ => _.Type == AppClaimNames.OwnDashboardId)
      .Select(c => Guid.Parse(c.Value));

  private static IEnumerable<Guid> GetJoinedDashboardIds(this ClaimsPrincipal self) =>
    self.Claims.Where(_ => _.Type == AppClaimNames.JoinedDashboardId)
      .Select(c => Guid.Parse(c.Value));
}