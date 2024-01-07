using System.Diagnostics;
using System.Net;

namespace Centurion.Monitor.Domain.Antibot;

public class ProtectionSolverHttpClientHandler : DelegatingHandler
{
  private readonly AntibotProtectionConfig _antibotConfig;
  private readonly IAntibotProtectionSolver _protectionSolver;
  private Cookie? _antibotCookie;

  public ProtectionSolverHttpClientHandler(HttpMessageHandler innerHandler, IAntibotProtectionSolver protectionSolver,
    AntibotProtectionConfig antibotConfig)
    : base(innerHandler)
  {
    _protectionSolver = protectionSolver;
    _antibotConfig = antibotConfig;
  }

  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
  {
    var resp = await TrySolveProtectionAsync(request, ct);
    if (resp != null)
    {
      return resp;
    }

    if (_antibotCookie != null)
    {
      request.Headers.Add("Cookie", _antibotCookie.ToString());
    }

    return await base.SendAsync(request, ct);
  }

  private async ValueTask<HttpResponseMessage?> TrySolveProtectionAsync(HttpRequestMessage request,
    CancellationToken ct)
  {
    if (request.RequestUri == null || !IsAntibotCookieExpiredOrEmpty)
    {
      return null;
    }

    using var solveActivity = Activity.Current?.Source.StartActivity("bot_protection_solve");
    _antibotCookie = await _protectionSolver.SolveCookieAsync(request.RequestUri, _antibotConfig, ct);

    bool protectedWithCaptcha = false;
    if (_antibotCookie != null)
    {
      var requestCopy = await request.CloneAsync(ct);

      requestCopy.Headers.Add("Cookie", _antibotCookie.ToString());
      var response = await base.SendAsync(requestCopy, ct);
      protectedWithCaptcha = await IsProtectedByCaptchaAsync(response);
      if (!protectedWithCaptcha)
      {
        return response;
      }
    }

    if (protectedWithCaptcha)
    {
      _antibotCookie = await _protectionSolver.SolveCaptchaAsync(request.RequestUri, _antibotConfig, ct);
    }

    return null;
  }

  private bool IsAntibotCookieExpiredOrEmpty
  {
    get
    {
      var untilExpire = _antibotCookie?.Expires - DateTime.UtcNow;
      return _antibotCookie == null || _antibotCookie.Expired || untilExpire < _antibotConfig.CookieLifetime / 2;
    }
  }

  private static ValueTask<bool> IsProtectedByCaptchaAsync(HttpResponseMessage response)
  {
    return ValueTask.FromResult(!response.IsSuccessStatusCode);
  }
}