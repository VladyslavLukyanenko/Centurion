using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Centurion.Cli.Core.Domain.Accounts;
using Centurion.Cli.Core.Domain.Profiles;
using Centurion.Cli.Core.Modules;
using Centurion.Cli.Core.Services.Accounts;
using Centurion.Cli.Core.Services.Profiles;
using Centurion.Cli.Core.Services.ToastNotifications;
using Centurion.Contracts;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels.Accounts;

public class GenerateAccountsWindowViewModel : ViewModelBase, IRoutableViewModel
{
  private readonly IAccountsRepository _accountsRepo;
  private readonly IToastNotificationManager _toasts;
  private readonly ReadOnlyObservableCollection<Account> _accounts;
  private readonly ReadOnlyObservableCollection<ProfileModel> _profiles;
  private CancellationTokenSource? _activeGenerationCts;

  public GenerateAccountsWindowViewModel(IModuleMetadataProvider modulesProvider, IAccountsRepository accountsRepo,
    IToastNotificationManager toasts, IProfilesRepository profilesRepository, IScreen hostScreen)
  {
    _accountsRepo = accountsRepo;
    _toasts = toasts;
    HostScreen = hostScreen;
    accountsRepo.Items.Connect()
      .Sort(SortExpressionComparer<Account>.Ascending(_ => _.Email))
      .Bind(out _accounts)
      .DisposeMany()
      .Subscribe();

    profilesRepository.Items.Connect()
      .TransformMany(_ => _.Profiles, _ => _.Id)
      .Sort(SortExpressionComparer<ProfileModel>.Ascending(_ => _.Name))
      .Bind(out _profiles)
      .DisposeMany()
      .Subscribe();

    // todo: add support of generators to backend
    // Modules = modulesProvider.SupportedModules
    //   .Where(_ => _.AccountGeneratorType != null)
    //   .ToArray();
    //
    // this.WhenAnyValue(_ => _.SelectedModule)
    //   .Select(module => module is null ? null : Locator.Current.GetService(module.AccountGeneratorType!))!
    //   .Cast<IAccountGenerator?>()
    //   .ToPropertyEx(this, _ => _.SelectedGenerator);
    //
    // this.WhenAnyValue(_ => _.NewAccountGroupName)
    //   .Where(n => !string.IsNullOrWhiteSpace(n))
    //   .Subscribe(_ => { SelectedAccountGroup = null; });

    this.WhenAnyValue(_ => _.SelectedModule)
      .Subscribe(_ => { GeneratedAccounts.Clear(); });

    // this.WhenAnyValue(_ => _.SelectedAccountGroup)
    //   .Where(g => g is not null)
    //   .Subscribe(_ => { NewAccountGroupName = null; });

    var falseResult = Observable.Return(false);
    var trueResult = Observable.Return(true);
    var canGenerate = this.WhenAnyValue(_ => _.SelectedGenerator)
      .Select(g =>
      {
        if (g is null)
        {
          return falseResult;
        }

        if (!g.ConfigurationFields.Any())
        {
          return trueResult;
        }

        return g.ConfigurationFields
          .Where(_ => _.IsRequired)
          .Select(_ => _.Changed)
          .CombineLatest(_ => _)
          .Select(
            fields => fields.Select(f => f.IsValid().ToObservable())
              .CombineLatest(_ => _.All(isValid => isValid))
          )
          .Switch();
      })
      .Switch()
      .CombineLatest(
        this.WhenAnyValue(_ => _.SelectedModule),
        this.WhenAnyValue(_ => _.ProfileRetrieveStrategySelected),
        this.WhenAnyValue(_ => _.Quantity).Select(q => q > 0),
        (isGeneratorValid, mod, strategySelected, isQtyValid) =>
          isGeneratorValid && mod != null && strategySelected && isQtyValid);
    //
    // this.WhenAnyValue(_ => _.PickRandomProfile)
    //   .CombineLatest(this.WhenAnyValue(_ => _.SelectedProfile), (b, _) => b)
    //   .Subscribe(_ => Reset());

    this.WhenAnyValue(_ => _.PickRandomProfile)
      .CombineLatest(this.WhenAnyValue(_ => _.SelectedProfile), (pick, profile) => pick || profile != null)
      .ToPropertyEx(this, _ => _.ProfileRetrieveStrategySelected);

    GenerateCommand = ReactiveCommand.CreateFromTask(GenerateAccountAsync, canGenerate);

    RemoveAccountCommand =
      ReactiveCommand.CreateFromTask<GeneratedAccount>(async (acc, ct) => await RemoveAccountAsync(acc, ct));

    CloseCommand = ReactiveCommand.CreateFromTask(async _ =>
    {
      Cancel();
      Reset();
      await hostScreen.Router.NavigateBack.Execute().FirstOrDefaultAsync();
    });

    CancelCommand = ReactiveCommand.Create(Cancel);
  }

  private void Cancel()
  {
    if (_activeGenerationCts is null)
    {
      return;
    }

    _activeGenerationCts.Cancel();
    _activeGenerationCts = null;
    _toasts.Show(ToastContent.Information("Cancelled"));
  }

  private Func<ProfileModel> SelectedProfileStrategy() => () => SelectedProfile!;

  private Func<ProfileModel> RoundRobinProfileStrategy()
  {
    var counter = 0;
    var profiles = _profiles.ToList();
    return () =>
    {
      var idx = counter++;
      if (idx > profiles.Count - 1)
      {
        counter = 0;
        idx = 0;
      }

      return profiles[idx];
    };
  }

  private async Task RemoveAccountAsync(GeneratedAccount acc, CancellationToken ct)
  {
    GeneratedAccounts.Remove(acc);
    await _accountsRepo.RemoveAsync(acc.Account, ct);
  }

  private async Task GenerateAccountAsync(CancellationToken cancelToken)
  {
    if (_activeGenerationCts is not null)
    {
      _toasts.Show(ToastContent.Warning("Account generation already running."));
      return;
    }

    _activeGenerationCts = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
    GeneratedAccounts.Clear();
    if (_profiles.Count == 0)
    {
      _toasts.Show(ToastContent.Error("No profiles found"));
      return;
    }

    try
    {
      _toasts.Show(ToastContent.Information("Accounts generation started."));
      var ct = _activeGenerationCts.Token;

      var strategy = PickRandomProfile
        ? RoundRobinProfileStrategy()
        : SelectedProfileStrategy();
      var generator = SelectedGenerator!;
      await generator.InitializeAsync(strategy, ct);
      await generator.PrepareAsync(ct);

      var requestedQty = Quantity;
      var generatedCount = 0D;
      Progress = 0;
      await foreach (var account in generator.GenerateAsync(ct).Take(requestedQty))
      {
        generatedCount++;
        GeneratedAccounts.Add(account);
        await _accountsRepo.SaveAsync(account.Account, ct);
        Progress = (int)Math.Round(generatedCount / requestedQty * 100);

        await generator.PrepareAsync(ct);
      }
    }
    catch (OperationCanceledException /* expected */)
    {
    }

    _activeGenerationCts = null;
    _toasts.Show(ToastContent.Success("Accounts generated successfully"));
  }

  private void Reset()
  {
    GeneratedAccounts.Clear();
    Quantity = 0;
    // NewAccountGroupName = null;
    SelectedModule = null;
    SelectedProfile = null;
    Progress = 0;
    // SelectedAccountGroup = null;
  }

  public bool ProfileRetrieveStrategySelected { [ObservableAsProperty] get; } = default!;

  [Reactive] public ModuleMetadata? SelectedModule { get; set; }

  [Reactive] public int Quantity { get; set; }

  // [Reactive] public Account? SelectedAccountGroup { get; set; }
  [Reactive] public ProfileModel? SelectedProfile { get; set; }
  [Reactive] public bool PickRandomProfile { get; set; }
  [Reactive] public int Progress { get; set; }
  public IAccountGenerator? SelectedGenerator { [ObservableAsProperty] get; } = null!;

  public IEnumerable<ModuleMetadata> Modules { get; }
  public ReadOnlyObservableCollection<Account> Accounts => _accounts;
  public ReadOnlyObservableCollection<ProfileModel> Profiles => _profiles;

  // [Reactive] public string? NewAccountGroupName { get; set; }
  public ReactiveCommand<Unit, Unit> GenerateCommand { get; }
  public ReactiveCommand<Unit, Unit> CancelCommand { get; }
  public ReactiveCommand<Unit, Unit> CloseCommand { get; }
  public ReactiveCommand<GeneratedAccount, Unit> RemoveAccountCommand { get; }

  [Reactive] public ObservableCollectionExtended<GeneratedAccount> GeneratedAccounts { get; set; } = new();
  public string UrlPathSegment => nameof(GenerateAccountsWindowViewModel);
  public IScreen HostScreen { get; }
}