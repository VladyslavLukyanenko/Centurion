using Centurion.Accounts.App.Model.Discord;
using Centurion.Accounts.Core.Products;
using CSharpFunctionalExtensions;

namespace Centurion.Accounts.Services;

public interface IDiscordAuthenticationHandler
{
  ValueTask<Result<AuthenticationResult>> AuthenticateAsync(Dashboard dashboard, SecurityToken token,
    CancellationToken ct = default);
}