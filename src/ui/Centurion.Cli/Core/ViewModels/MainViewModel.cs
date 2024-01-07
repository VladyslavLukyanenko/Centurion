using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls.Mixins;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Services.ToastNotifications;
using Centurion.Cli.Core.ViewModels.Accounts;
using Centurion.Cli.Core.ViewModels.Harvesters;
using Centurion.Cli.Core.ViewModels.Home;
using Centurion.Cli.Core.ViewModels.Profiles;
using Centurion.Cli.Core.ViewModels.Proxies;
using Centurion.Cli.Core.ViewModels.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace Centurion.Cli.Core.ViewModels;

public class MainViewModel : ViewModelBase, IScreen
{
#if DEBUG
  public MainViewModel()
  {
  }
#endif

  public MainViewModel(RoutingState router, IIdentityService identityService, UpdateViewModel update,
    IToastNotificationManager toasts, Lazy<SettingsViewModel> settings, NotificationsViewModel notifications,
    IEnumerable<IExecutionStatusProvider> executionStatusProviders)
  {
    Router = router;
    Update = update;
    Notifications = notifications;
    identityService.User
      .Select(u => u?.Avatar)
      .ToPropertyEx(this, _ => _.UserPicture);

    LogOutCommand = ReactiveCommand.Create(identityService.LogOut);
    CheckForUpdatesCommand = ReactiveCommand.Create(() =>
    {
      update.CheckForUpdatesCommand.Execute()
        .Subscribe(isUpdateAvailable =>
        {
          if (!isUpdateAvailable)
          {
            toasts.Show(ToastContent.Information("No updates available." /* You're using the latest version.*/));
          }
        });
    });

    NavigateToPageCommand = ReactiveCommand.CreateFromTask<IRoutableViewModel, Unit>(async viewModel =>
    {
      await Router.Navigate.Execute(viewModel).FirstOrDefaultAsync();
      return Unit.Default;
    });

    NavigateToSettingsCommand =
      ReactiveCommand.CreateFromObservable(() => NavigateToPageCommand.Execute(settings.Value));

    executionStatusProviders.Select(_ => _.IsFetching)
      .CombineLatest()
      .Select(isFetching => isFetching.Any(fetching => fetching))
      .ToPropertyEx(this, _ => _.IsBusy)
      .DisposeWith(Disposable);
  }

  public IList<NavItemViewModel> MainNavigation { get; } = new[]
  {
    NavItemViewModel.Create<HomeViewModel>(),
    NavItemViewModel.Create<TasksViewModel>(),
    NavItemViewModel.Create<ProxiesViewModel>(),
    NavItemViewModel.Create<AccountsViewModel>(),
    NavItemViewModel.Create<ProfilesViewModel>(),
    NavItemViewModel.Create<HarvestersViewModel>(),
  };

  public bool IsBusy { [ObservableAsProperty] get; }
  public string? UserPicture { [ObservableAsProperty] get; }
  public RoutingState Router { get; }
  public UpdateViewModel Update { get; }
  public NotificationsViewModel Notifications { get; }
  public ReactiveCommand<Unit, Unit> LogOutCommand { get; }
  public ReactiveCommand<Unit, Unit> CheckForUpdatesCommand { get; }
  public ReactiveCommand<IRoutableViewModel, Unit> NavigateToPageCommand { get; private set; }
  public ReactiveCommand<Unit, Unit> NavigateToSettingsCommand { get; private set; }
}

public class NavItemViewModel : ViewModelBase
{
  public static NavItemViewModel Create<T>(string? title = null) where T : IRoutableViewModel => new()
  {
    Title = title ?? typeof(T).Name.Replace("ViewModel", ""),
    PageType = typeof(T)
  };

  public string Title { get; init; }
  public Type PageType { get; init; }

  public IRoutableViewModel ViewModel => (IRoutableViewModel)Locator.Current.GetService(PageType)!;
}