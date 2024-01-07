using Centurion.Accounts.Foundation.Config;
using Centurion.Accounts.Foundation.Mvc;
using Centurion.Accounts.Foundation.Mvc.Controllers;
using Centurion.Accounts.Services;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Centurion.Accounts.Core.Products.Services;

namespace Centurion.Accounts.Products.Controllers;

[ApiV1HttpRoute("Bots/LicenseKeys")]
public class BotsLicenseKeysController : SoftwareDashboardBoundControllerBase
{
  private readonly SsoConfig _ssoConfig;
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly ILicenseKeyRepository _licenseKeyRepository;

  public BotsLicenseKeysController(IServiceProvider provider, SsoConfig ssoConfig,
    IHttpClientFactory httpClientFactory, ILicenseKeyRepository licenseKeyRepository)
    : base(provider)
  {
    _ssoConfig = ssoConfig;
    _httpClientFactory = httpClientFactory;
    _licenseKeyRepository = licenseKeyRepository;
  }

  [HttpPatch("reset")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async ValueTask<IActionResult> ResetAsync(CancellationToken ct)
  {
    var key = await _licenseKeyRepository.GetByValueAsync(CurrentLicenseKey, ct);
    if (key == null)
    {
      return NotFound();
    }

    key.ResetSession();
    _licenseKeyRepository.Update(key);

    return NoContent();
  }

  [AllowAnonymous]
  [HttpPost("login")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async ValueTask<IActionResult> LoginWithKeyAsync(string key, long productId, string hwid, CancellationToken ct)
  {
    var client = _httpClientFactory.CreateClient();
    var disco = await client.GetDiscoveryDocumentAsync(_ssoConfig.AuthorityUrl, ct);

    var token = await client.RequestTokenAsync(new TokenRequest
    {
      Address = disco.TokenEndpoint,
      GrantType = LicenseKeyGrantValidator.GrantTypeName,

      ClientId = "licensekey-auth.client",
      ClientSecret = "secret",

      Parameters =
      {
        {"key", key},
        {"pid", productId.ToString()},
        {"hwid", hwid},
      }
    }, ct);

    return Ok(token.Json);
  }
}