using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Avalonia.Controls.Mixins;
using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Services.ToastNotifications;
using Centurion.Cli.Core.Validators;
using Centurion.Contracts;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels;

public class SettingsViewModel : PageViewModelBase, IRoutableViewModel
{
  private readonly IDiscordSettingsService _discordSettingsService;
  private readonly DiscordSettingsValidator _discordSettingsValidator;
  private readonly IToastNotificationManager _toasts;
  private readonly IWebHookManager _webHookManager;

#if DEBUG
  public SettingsViewModel()
    : base("", null)
  {
  }
#endif

  public SettingsViewModel(IDiscordSettingsService discordSettingsService,
    IScreen hostScreen, DiscordSettingsValidator discordSettingsValidator, IToastNotificationManager toasts,
    IWebHookManager webHookManager, IMessageBus messageBus, IIdentityService identityService,
    IGeneralSettingsService generalSettingsService, IDialogService dialogService)
    : base("Settings", messageBus)
  {
    _discordSettingsService = discordSettingsService;
    _discordSettingsValidator = discordSettingsValidator;
    _toasts = toasts;
    _webHookManager = webHookManager;
    HostScreen = hostScreen;

    generalSettingsService.Settings.ToPropertyEx(this, _ => _.GeneralSettings);

    this.WhenAnyValue(_ => _.GeneralSettings)
      .Select(_ => _.Changed)
      .Switch()
      .Throttle(TimeSpan.FromMilliseconds(500))
      .Select(_ => generalSettingsService.Save(GeneralSettings!).AsTask().ToObservable())
      .Switch()
      .Subscribe()
      .DisposeWith(Disposable);

    identityService.User.ToPropertyEx(this, _ => _.User);
    identityService.User
      .Select(_ => _?.Expiry is null ? null : (int?)(DateTimeOffset.UtcNow - _.Expiry.Value).TotalDays)
      .ToPropertyEx(this, _ => _.UserExpiryTotalDays);
    LogOutCommand = ReactiveCommand.Create(identityService.LogOut);

    discordSettingsService.Settings.ToPropertyEx(this, _ => _.DiscordSettings);
    this.WhenAnyValue(_ => _.DiscordSettings)
      .DistinctUntilChanged()
      .Subscribe(settings =>
      {
        SuccessWebHookUrl = settings?.SuccessUrl;
        FailureWebHookUrl = settings?.FailureUrl;
      });


    this.WhenAnyValue(_ => _.SuccessWebHookUrl)
      .CombineLatest(this.WhenAnyValue(_ => _.FailureWebHookUrl))
      .Throttle(TimeSpan.FromMilliseconds(500))
      .ObserveOn(RxApp.TaskpoolScheduler)
      .Do(_ => SaveDiscordSettingsAsync().ToObservable())
      .Subscribe();

    TestSuccessWebhookCommand =
      ReactiveCommand.CreateFromTask(async () => await SendTestWebhookAsync(DiscordSettings?.SuccessUrl));

    TestFailureWebhookCommand =
      ReactiveCommand.CreateFromTask(async () => await SendTestWebhookAsync(DiscordSettings?.FailureUrl));

    ChangeCheckoutSoundCommand = ReactiveCommand.CreateFromTask(async ct =>
    {
      var mp3File = await dialogService.PickOpenFileAsync("Please select Checkout Sound", ".mp3");
      if (string.IsNullOrEmpty(mp3File))
      {
        _toasts.Show(ToastContent.Error("File was not selected"));
        return;
      }

      await generalSettingsService.UpdateCheckoutSound(mp3File, ct);
      _toasts.Show(ToastContent.Success("Checkout Sound was changed"));
    });

    ChangeDeclineSoundCommand = ReactiveCommand.CreateFromTask(async ct =>
    {
      var mp3File = await dialogService.PickOpenFileAsync("Please select Decline Sound", ".mp3");
      if (string.IsNullOrEmpty(mp3File))
      {
        _toasts.Show(ToastContent.Error("File was not selected"));
        return;
      }

      await generalSettingsService.UpdateDeclineSound(mp3File, ct);
      _toasts.Show(ToastContent.Success("Decline Sound was changed"));
    });

    ResetCheckoutSoundCommand = ReactiveCommand.Create(() =>
    {
      generalSettingsService.ResetCheckoutSound();
      _toasts.Show(ToastContent.Success("Checkout Sound was reset"));
    });

    ResetDeclineSoundCommand = ReactiveCommand.Create(() =>
    {
      generalSettingsService.ResetDeclineSound();
      _toasts.Show(ToastContent.Success("Decline Sound was reset"));
    });

    TestCheckoutSoundCommand =
      ReactiveCommand.CreateFromTask(async ct => await generalSettingsService.PlayCheckoutSound(ct));

    TestDeclineSoundCommand =
      ReactiveCommand.CreateFromTask(async ct => await generalSettingsService.PlayDeclineSound(ct));
  }

  private async Task SendTestWebhookAsync(string? url)
  {
    if (string.IsNullOrEmpty(url))
    {
      _toasts.Show(ToastContent.Error("Webhook URL is empty"));
      return;
    }

    ToastContent content;
    if (await _webHookManager.TestWebhook(url))
    {
      content = ToastContent.Success("Test webhook sent successfully");
    }
    else
    {
      content = ToastContent.Error("Webhook wasn't sent. Please check provided url");
    }

    _toasts.Show(content);
  }

  private async Task SaveDiscordSettingsAsync()
  {
    if (DiscordSettings == null || SuccessWebHookUrl == DiscordSettings.SuccessUrl
        && FailureWebHookUrl == DiscordSettings.FailureUrl)
    {
      return;
    }

    var result = await _discordSettingsValidator.ValidateAsync(new WebhookSettings
    {
      SuccessUrl = SuccessWebHookUrl ?? "",
      FailureUrl = FailureWebHookUrl ?? ""
    });
    if (!result.IsValid)
    {
      _toasts.Show(ToastContent.Error(result.ToString()));
      return;
    }

    DiscordSettings.SuccessUrl = SuccessWebHookUrl;
    DiscordSettings.FailureUrl = FailureWebHookUrl;
    await _discordSettingsService.UpdateAsync(DiscordSettings);
    _toasts.Show(ToastContent.Success("Webhooks saved"));
  }

  public GeneralSettings GeneralSettings { [ObservableAsProperty] get; }
  public WebhookSettings? DiscordSettings { [ObservableAsProperty] get; } = null!;

  public int? UserExpiryTotalDays { [ObservableAsProperty] get; }
  public User? User { [ObservableAsProperty] get; }
  [Reactive] public string? SuccessWebHookUrl { get; set; }
  [Reactive] public string? FailureWebHookUrl { get; set; }

  public ReactiveCommand<Unit, Unit> LogOutCommand { get; }
  public ReactiveCommand<Unit, Unit> TestSuccessWebhookCommand { get; set; }
  public ReactiveCommand<Unit, Unit> TestFailureWebhookCommand { get; set; }


  public string UrlPathSegment => nameof(SettingsViewModel);
  public IScreen HostScreen { get; }

  public string AppVersionStr => AppInfo.ProductFullName;

  public ReactiveCommand<Unit, Unit> ChangeCheckoutSoundCommand { get; }
  public ReactiveCommand<Unit, Unit> ResetCheckoutSoundCommand { get; }
  public ReactiveCommand<Unit, Unit> TestCheckoutSoundCommand { get; }

  public ReactiveCommand<Unit, Unit> ChangeDeclineSoundCommand { get; }
  public ReactiveCommand<Unit, Unit> ResetDeclineSoundCommand { get; }
  public ReactiveCommand<Unit, Unit> TestDeclineSoundCommand { get; }
}