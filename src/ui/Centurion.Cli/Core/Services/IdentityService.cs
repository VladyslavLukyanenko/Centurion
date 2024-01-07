using System.Reactive.Linq;
using System.Reactive.Subjects;
using Centurion.Cli.Core.Clients;
using Centurion.Cli.Core.Domain;
using ReactiveUI;

namespace Centurion.Cli.Core.Services;

public class IdentityService : IIdentityService
{
  private readonly ITokenClient _tokenClient;
  private readonly IDeviceInfoProvider _deviceInfoProvider;
  private readonly ILicenseKeyProvider _licenseKeyProvider;
  private readonly ISoftwareInfoProvider _softwareInfoProvider;
  private readonly BehaviorSubject<User?> _user = new(null);
  private readonly ITokenStore _tokenStore;

  public IdentityService(ITokenClient tokenClient, IDeviceInfoProvider deviceInfoProvider,
    ILicenseKeyProvider licenseKeyProvider, ISoftwareInfoProvider softwareInfoProvider, ITokenStore tokenStore)
  {
    _tokenClient = tokenClient;
    _deviceInfoProvider = deviceInfoProvider;
    _licenseKeyProvider = licenseKeyProvider;
    _softwareInfoProvider = softwareInfoProvider;
    _tokenStore = tokenStore;

    User = _user
      .ObserveOn(RxApp.MainThreadScheduler);

    IsAuthenticated = User.Select(user => user != null);
  }

  public User? CurrentUser => _user.Value;
  public IObservable<User?> User { get; }
  public IObservable<bool> IsAuthenticated { get; }

  public void Authenticate(AuthenticationResult? result)
  {
    if (result?.IsSuccess == true)
    {
      _softwareInfoProvider.SetSoftwareVersion(result.SoftwareVersion!);
      var user = new User(result.DiscordId, result.UserName!, result.Discriminator, result.ExpiresAt, result.Avatar!);
      _user.OnNext(user);
      _tokenStore.UseToken(result.AccessToken!.RawAccessToken);
    }
    else
    {
      Invalidate();
    }
  }

  public async ValueTask<AuthenticationResult?> TryAuthenticateAsync(CancellationToken ct = default)
  {
    var result = await FetchIdentityAsync(ct);
    Authenticate(result);

    return result;
  }

  public async ValueTask<AuthenticationResult?> FetchIdentityAsync(CancellationToken ct = default)
  {
    var licenseKey = _licenseKeyProvider.CurrentLicenseKey;
    if (string.IsNullOrEmpty(licenseKey))
    {
      return AuthenticationResult.CreateUnknownError();
    }

    var hwid = await _deviceInfoProvider.GetHwidAsync(ct);
    return await _tokenClient.FetchTokenAsync(licenseKey, hwid, ct);
  }

  public void LogOut()
  {
    Invalidate();
  }

  public ValueTask DeactivateAsync(CancellationToken ct = default)
  {
    if (string.IsNullOrEmpty(_licenseKeyProvider.CurrentLicenseKey))
    {
      return default;
    }

    // await _tokenClient.DeactivateOnCurrentDeviceAsync(_licenseKeyProvider.CurrentLicenseKey, ct);
    LogOut();
    throw new NotImplementedException();
  }

  private void Invalidate()
  {
    _licenseKeyProvider.Invalidate();
    _user.OnNext(null);
  }
}