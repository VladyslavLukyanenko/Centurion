namespace Centurion.Cli.Core.Clients;

public interface ITokenClient
{
  ValueTask<AuthenticationResult> FetchTokenAsync(string licenseKey, string hwid, CancellationToken ct = default);
}