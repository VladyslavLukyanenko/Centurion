using Centurion.Accounts.App.Identity.Model;
using Centurion.Accounts.App.Identity.Services;
using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.Foundation.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Centurion.Accounts.Core.Identity.Services;
using Centurion.Accounts.Foundation.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Centurion.Accounts.Controllers;

[Authorize(Roles = SupportedRoleNames.Administrator)]
public class UsersController : SecuredControllerBase
{
  private readonly IUserProvider _userProvider;
  private readonly IUserRepository _userRepository;
  private readonly IIdentityService _identityService;

  public UsersController(IServiceProvider serviceProvider, IUserProvider userProvider, IUserRepository userRepository,
    IIdentityService identityService)
    : base(serviceProvider)
  {
    _userProvider = userProvider;
    _userRepository = userRepository;
    _identityService = identityService;
  }

  [HttpGet("{userId:long}")]
  public async ValueTask<IActionResult> GetUserById(long userId, CancellationToken ct)
  {
    var user = await _userProvider.GetUserByIdAsync(userId, ct);
    if (user == null)
    {
      return NotFound();
    }

    return Ok(user);
  }

  [HttpGet("{userId:long}/lock")]
  public async ValueTask<IActionResult> LockUserById(long userId, CancellationToken ct)
  {
    var user = await _userRepository.GetByIdAsync(userId, ct);
    if (user == null)
    {
      return NotFound();
    }

    user.ToggleLockOut(true);
    return Ok();
  }

  [HttpGet("{userId:long}/unlock")]
  public async ValueTask<IActionResult> UnlockUserById(long userId, CancellationToken ct)
  {
    var user = await _userRepository.GetByIdAsync(userId, ct);
    if (user == null)
    {
      return NotFound();
    }

    user.ToggleLockOut(false);
    return Ok();
  }

  [HttpDelete("{userId:long}")]
  public async ValueTask<IActionResult> DeleteUserById(long userId, CancellationToken ct)
  {
    var user = await _userRepository.GetByIdAsync(userId, ct);
    if (user == null)
    {
      return NotFound();
    }

    user.Remove();
    return NoContent();
  }

  [HttpPut("{userId:long}")]
  public async ValueTask<IActionResult> UpdateAsync(long userId, [FromBody] UserData data, CancellationToken ct)
  {
    var user = await _userRepository.GetByIdAsync(userId, ct);
    if (user == null)
    {
      return NotFound();
    }

    await _identityService.UpdateAsync(user, data, ct);
    return NoContent();
  }
}