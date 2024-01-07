using System.Security.Claims;
using Centurion.Accounts.App.Model.Discord;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.Services;

public interface IClaimsProvider
{
  IList<Claim> GetDashboardClaims(Dashboard dashboard);
  IList<Claim> GetUserClaims(User user);
  ValueTask<IList<Claim>> GetPermissionClaimsAsync(User user, Dashboard dashboard);
  IList<Claim> GetSecurityTokenClaims(SecurityToken token);
}