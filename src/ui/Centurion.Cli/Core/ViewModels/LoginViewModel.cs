using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Services.ToastNotifications;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels;

[DataContract]
public class LoginViewModel : ViewModelBase//, IRoutableViewModel
{

#if DEBUG
  public LoginViewModel()
  {
  }
#endif

  public LoginViewModel(IScreen hostScreen, ILicenseKeyProvider licenseKeyProvider, IIdentityService identityService,
    ILicenseKeyStore licenseKeyStore, IToastNotificationManager toasts, UpdateViewModel update)
  {
    HostScreen = hostScreen;
    Update = update;

    Login = ReactiveCommand.CreateFromTask(async ct =>
    {
      var licenseKey = LicenseKey;
      if (string.IsNullOrEmpty(licenseKey))
      {
        toasts.Show(ToastContent.Error("Can't authenticate", "Please enter your key."));
        return;
      }

      licenseKeyProvider.UseLicenseKey(licenseKey);
      var authResult = await identityService.FetchIdentityAsync(ct);
      if (authResult?.IsSuccess == true)
      {
        ErrorMessage = string.Empty;
        await licenseKeyStore.StoreKeyAsync(licenseKey, ct);
        identityService.Authenticate(authResult);
        toasts.Show(ToastContent.Success($"Welcome, {identityService.CurrentUser!.FullUserName}", "Authenticated"));
        Update.PreventCancellation = await Update.CheckForUpdatesCommand.Execute().FirstOrDefaultAsync();
      }
      else
      {
        ErrorMessage = authResult?.Message ?? "Can't authenticate. Something went wrong";
        toasts.Show(ToastContent.Error(authResult?.Message ?? "Can't authenticate", "Authentication failed."));
      }
    });

    LoadStoredKeyCommand = ReactiveCommand.CreateFromTask(async ct =>
    {
      string? key = await licenseKeyStore.GetStoredKeyAsync(ct);
      if (!string.IsNullOrEmpty(key))
      {
        LicenseKey = key;
      }
    });
  }

  [Reactive, DataMember] public string LicenseKey { get; set; } = string.Empty;
  [Reactive, DataMember] public string ErrorMessage { get; set; } = string.Empty;

  [IgnoreDataMember] public ReactiveCommand<Unit, Unit> Login { get; }
  [IgnoreDataMember] public ReactiveCommand<Unit, Unit> LoadStoredKeyCommand { get; }

  public string UrlPathSegment => nameof(LoginViewModel);
  public IScreen HostScreen { get; }
  public UpdateViewModel Update { get; }
}