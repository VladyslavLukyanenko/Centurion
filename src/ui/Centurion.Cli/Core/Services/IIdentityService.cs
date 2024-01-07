using Centurion.Cli.Core.Clients;
using Centurion.Cli.Core.Domain;

namespace Centurion.Cli.Core.Services;

public interface IIdentityService
{
  User? CurrentUser { get; }
  IObservable<User?> User { get; }
  IObservable<bool> IsAuthenticated { get; }
  ValueTask<AuthenticationResult?> TryAuthenticateAsync(CancellationToken ct = default);
  ValueTask<AuthenticationResult?> FetchIdentityAsync(CancellationToken ct = default);
  void Authenticate(AuthenticationResult? result);
  void LogOut();
  ValueTask DeactivateAsync(CancellationToken ct = default);
}