using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Centurion.Cli.Core.Domain.Profiles;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Services.Profiles;
using Centurion.Cli.Core.Services.ToastNotifications;
using Centurion.Cli.Core.Validators;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels.Profiles;

public class ProfileEditorViewModel : ViewModelBase, IRoutableViewModel
{
  private ProfileGroupModel? _group;
#if DEBUG
  public ProfileEditorViewModel()
  {
  }
#endif

  public ProfileEditorViewModel(ICountriesService countriesService, IProfilesRepository profilesRepository,
    AddressValidator addressValidator, ProfileValidator profileValidator, IToastNotificationManager toasts,
    IScreen hostScreen)
  {
    var profile = this.WhenAnyValue(_ => _.Profile);
    CompositeDisposable? disposable = null;
    profile
      .Where(p => p != null)
      .Subscribe(p =>
      {
        disposable?.Dispose();
        disposable = new CompositeDisposable();

        ShippingAddress = new AddressEditorViewModel(countriesService, p!.ShippingAddress, addressValidator);
        BillingAddress = new AddressEditorViewModel(countriesService, p.BillingAddress, addressValidator);
        p.Changed
          .Throttle(TimeSpan.FromMilliseconds(200))
          .Select(_ => profileValidator.Validate(Profile).IsValid)
          .ToPropertyEx(this, _ => _.IsValid)
          .DisposeWith(disposable);

        p.Changed.Select(_ => p.Billing != null)
          .ToPropertyEx(this, _ => _.IsCreditCardCreated)
          .DisposeWith(disposable);
        
        disposable.Add(ShippingAddress);
        disposable.Add(BillingAddress);
        p.RaisePropertyChanged();
      })
      .DisposeWith(Disposable);

    // CancelCommand = ReactiveCommand.Create(() =>
    //   {
    //     Profile = null!;
    //     _group = null;
    //     HostScreen!.Router.NavigateBack.Execute().Subscribe();
    //   })
    //   .DisposeWith(Disposable);

    var canExecuteSave = new BehaviorSubject<bool>(false);
    SaveChangesCommand = ReactiveCommand.CreateFromTask(async ct =>
      {
        var validationResult = await profileValidator.ValidateAsync(Profile, ct);
        if (!validationResult.IsValid)
        {
          toasts.Show(ToastContent.Error(validationResult.ToString()));
          return;
        }

        if (!_group!.Profiles.Contains(Profile))
        {
          _group!.Profiles.Add(Profile);
        }

        await profilesRepository.SaveSilentlyAsync(_group!, ct);
        Profile = new ProfileModel();
        // await CancelCommand.Execute().FirstOrDefaultAsync();
        toasts.Show(ToastContent.Success("Changes made to profile were saved"));
      }, canExecuteSave)
      .DisposeWith(Disposable);

    RemoveCreditCard = ReactiveCommand.Create(() => { Profile.Billing = null; }).DisposeWith(Disposable);
    AddCreditCard = ReactiveCommand.Create(() => { Profile.Billing = new BillingModel(); }).DisposeWith(Disposable);

    ToggleCvvVisibilityCommand = ReactiveCommand.Create(() => { IsCvvVisible = !IsCvvVisible; })
      .DisposeWith(Disposable);
    ToggleBillingCommand =
      ReactiveCommand.CreateFromObservable(() => (IsCreditCardCreated ? RemoveCreditCard : AddCreditCard).Execute())
        .DisposeWith(Disposable);

    this.WhenAnyValue(_ => _.IsValid)
      .ObserveOn(RxApp.MainThreadScheduler)
      .CombineLatest(SaveChangesCommand.IsExecuting, (isValid, isExecuting) => (isValid, isExecuting))
      .Subscribe(p => canExecuteSave.OnNext(p.isValid && !p.isExecuting))
      .DisposeWith(Disposable);
    HostScreen = hostScreen;
  }

  public void EditProfile(ProfileGroupModel group, ProfileModel profile)
  {
    Profile = profile;
    _group = group;
  }

  public bool IsCreditCardCreated { [ObservableAsProperty] get; } = default!;
  public bool IsValid { [ObservableAsProperty] get; } = default!;
  [Reactive] public ProfileModel Profile { get; private set; } = null!;
  [Reactive] public AddressEditorViewModel ShippingAddress { get; set; } = null!;
  [Reactive] public AddressEditorViewModel BillingAddress { get; set; } = null!;

  [Reactive] public bool IsCvvVisible { get; private set; }


  public string UrlPathSegment => nameof(ProfileEditorViewModel);
  public IScreen HostScreen { get; }


  // public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }
  public ReactiveCommand<Unit, Unit> SaveChangesCommand { get; private set; }
  public ReactiveCommand<Unit, Unit> ToggleCvvVisibilityCommand { get; private set; }
  public ReactiveCommand<Unit, Unit> ToggleBillingCommand { get; private set; }
  public ReactiveCommand<Unit, Unit> AddCreditCard { get; private set; }
  public ReactiveCommand<Unit, Unit> RemoveCreditCard { get; private set; }
}