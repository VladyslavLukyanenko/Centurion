using System.Security.Claims;
using Centurion.TaskManager.Core.Services;

namespace Centurion.TaskManager.Web.Services;

public interface IUserInfoFactory
{
  UserInfo Create(ClaimsPrincipal principal);
}