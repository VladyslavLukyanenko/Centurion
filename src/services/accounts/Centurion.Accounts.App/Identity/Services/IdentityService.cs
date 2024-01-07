using Centurion.Accounts.App.Identity.Model;
using Microsoft.AspNetCore.Identity;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Identity.Services;

namespace Centurion.Accounts.App.Identity.Services;

public class IdentityService : IIdentityService
{
  private readonly UserManager<User> _userManager;
  private readonly IUserRepository _userRepository;
  private readonly IUserService _userService;

  public IdentityService(IUserService userService, IUserRepository userRepository, UserManager<User> userManager)
  {
    _userService = userService;
    _userRepository = userRepository;
    _userManager = userManager;
  }
  //
  // public async Task<long> RegisterAsync(RegisterWithEmailCommand cmd, CancellationToken ct = default)
  // {
  //   var user = await _userService.RegisterAsync(Core.Primitives.Email.CreateUnconfirmed(cmd.Email), cmd.Password,
  //     cmd.Name, ct);
  //   return user.Id;
  // }
  //
  // public async Task<string> ConfirmEmailAsync(ConfirmEmailCommand cmd, CancellationToken ct = default)
  // {
  //   var user = await _userManager.FindByEmailAsync(cmd.Email);
  //   var confirmationResult = await _userManager.ConfirmEmailAsync(user, cmd.ConfirmationCode);
  //   if (!confirmationResult.Succeeded)
  //   {
  //     return string.Join("\n", confirmationResult.Errors.Select(r => r.Description));
  //   }
  //
  //   return null;
  // }
  //
  // public async Task<long> CreateConfirmedAsync(CreateWithConfirmedEmailCommand cmd, CancellationToken ct = default)
  // {
  //   var user = await _userService.CreateConfirmed(cmd.Email, cmd.Password, cmd.RoleNames, cmd.Name, ct);
  //   return user.Id;
  // }

  public Task UpdateAsync(User user, UserData data, CancellationToken ct = default)
  {
    user.Name = data.Name;
    user.SetEmail(data.Email, data.IsEmailConfirmed);
    user.ToggleLockOut(data.IsLockedOut);

    _userRepository.Update(user);
    return Task.CompletedTask;
  }
}