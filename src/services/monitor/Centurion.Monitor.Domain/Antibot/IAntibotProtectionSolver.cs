using System.Net;

namespace Centurion.Monitor.Domain.Antibot;

public interface IAntibotProtectionSolver
{
  string ProviderName { get; }

  ValueTask<Cookie?> SolveCookieAsync(Uri requestUri, AntibotProtectionConfig config, CancellationToken ct = default);

  ValueTask<Cookie?> SolveCaptchaAsync(Uri requestUri, AntibotProtectionConfig config,
    CancellationToken ct = default);
}