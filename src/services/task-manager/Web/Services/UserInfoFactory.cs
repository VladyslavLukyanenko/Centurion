using System.Security.Claims;
using Centurion.Accounts.Foundation.Authorization;
using Centurion.SeedWork.Web;
using Centurion.TaskManager.Core.Services;
using IdentityModel;

namespace Centurion.TaskManager.Web.Services;

public class UserInfoFactory : IUserInfoFactory
{
  public UserInfo Create(ClaimsPrincipal principal)
  {
    return new UserInfo
    {
      Avatar = principal.FindFirstValue(AppClaimNames.DiscordAvatar),
      DiscordId = ulong.Parse(principal.FindFirstValue(AppClaimNames.DiscordId)),
      UserId = principal.GetUserId()!,
      UserName = principal.FindFirstValue(JwtClaimTypes.Name)
    };
  }
}