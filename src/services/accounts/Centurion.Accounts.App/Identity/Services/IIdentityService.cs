using Centurion.Accounts.App.Identity.Model;
using Centurion.Accounts.Core.Identity;

namespace Centurion.Accounts.App.Identity.Services;

public interface IIdentityService
{
  // Task<long> RegisterAsync(RegisterWithEmailCommand cmd, CancellationToken ct = default);
  // Task<string> ConfirmEmailAsync(ConfirmEmailCommand cmd, CancellationToken ct = default);

  // Task<long> CreateConfirmedAsync(CreateWithConfirmedEmailCommand cmd, CancellationToken ct = default);
  Task UpdateAsync(User user, UserData data, CancellationToken ct = default);
}