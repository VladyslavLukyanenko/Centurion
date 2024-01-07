using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Centurion.Accounts.Foundation.Authorization;

namespace Centurion.Accounts.Foundation.Mvc.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public abstract class SecuredControllerBase : ControllerBase
{
  private long? _currentUserId;

  protected SecuredControllerBase(IServiceProvider provider)
  {
  }

  protected long CurrentUserId => _currentUserId ??= User.GetUserId();
}