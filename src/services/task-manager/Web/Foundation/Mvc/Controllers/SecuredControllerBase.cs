using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Centurion.TaskManager.Web.Foundation.Mvc.Controllers
{
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  public abstract class SecuredControllerBase
    : ControllerBase
  {
    private readonly Lazy<long> _currentUserIdProvider;
    protected readonly IAuthorizationService AuthorizationService;

    protected SecuredControllerBase(IServiceProvider provider)
    {
      _currentUserIdProvider = new Lazy<long>(() => long.Parse(User.FindFirst("id")!.Value));
      AuthorizationService = provider.GetRequiredService<IAuthorizationService>();
    }

    public long CurrentUserId => _currentUserIdProvider.Value;
  }
}