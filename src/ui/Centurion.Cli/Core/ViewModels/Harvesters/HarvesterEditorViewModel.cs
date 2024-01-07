using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls.Mixins;
using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Domain.Accounts;
using Centurion.Cli.Core.Modules;
using Centurion.Cli.Core.Services.Accounts;
using Centurion.Cli.Core.Services.Harvesters;
using Centurion.Cli.Core.Services.Proxies;
using Centurion.Contracts;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels.Harvesters;

public class HarvesterEditorViewModel : ViewModelBase, IRoutableViewModel
{
  readonly ReadOnlyObservableCollection<Account> _accounts;
  readonly ReadOnlyObservableCollection<ProxyGroup> _proxyGroups;

  public HarvesterEditorViewModel(IScreen hostScreen, IModuleMetadataProvider moduleProvider,
    IProxyGroupsRepository proxyGroupsRepo, IAccountsRepository accountsRepo, IHarvestersRepository harvestersRepo)
  {
    HostScreen = hostScreen;
    SupportedModules = moduleProvider.SupportedModules.ToList();

    accountsRepo.Items.Connect()
      .SortBy(_ => _.Email)
      .Bind(out _accounts)
      .Subscribe()
      .DisposeWith(Disposable);

    proxyGroupsRepo.Items.Connect()
      .SortBy(_ => _.Name)
      .Bind(out _proxyGroups)
      .Subscribe()
      .DisposeWith(Disposable);

    this.WhenAnyValue(_ => _.EditingHarvester)
      .WhereNotNull()
      .Subscribe(h =>
      {
        SelectedAccount = _accounts.FirstOrDefault(_ => _.Id == h.AccountId);
        SelectedModule = SupportedModules.FirstOrDefault(_ => _.Module == h.Module);
        SelectedGroup = _proxyGroups.FirstOrDefault(_ => _.Proxies.Any(p => p.Id == h.ProxyId));
        SelectedProxy = SelectedGroup?.Proxies.FirstOrDefault(it => it.Id == h.ProxyId);
      })
      .DisposeWith(Disposable);

    this.WhenAnyValue(_ => _.SelectedProxy)
      .WhereNotNull()
      .Where(_ => EditingHarvester is not null)
      .Subscribe(p => EditingHarvester!.ProxyId = p.Id)
      .DisposeWith(Disposable);

    this.WhenAnyValue(_ => _.SelectedModule)
      .WhereNotNull()
      .Where(_ => EditingHarvester is not null)
      .Subscribe(p => EditingHarvester!.Module = p.Module)
      .DisposeWith(Disposable);

    this.WhenAnyValue(_ => _.SelectedAccount)
      .WhereNotNull()
      .Where(_ => EditingHarvester is not null)
      .Subscribe(p => EditingHarvester!.AccountId = p.Id)
      .DisposeWith(Disposable);


    var isAccNotNull = this.WhenAnyValue(_ => _.SelectedAccount).Select(it => it is not null);
    var isModuleNotNull = this.WhenAnyValue(_ => _.SelectedModule).Select(it => it is not null);
    var isProxyNotNull = this.WhenAnyValue(_ => _.SelectedProxy).Select(it => it is not null);

    var canSave = isAccNotNull.CombineLatest(isModuleNotNull, isProxyNotNull, (acc, mod, prox) => acc && mod && prox);
    SaveCommand = ReactiveCommand.CreateFromTask(async ct =>
    {
      await harvestersRepo.SaveAsync(EditingHarvester!, ct);
      EditingHarvester = new HarvesterModel();
    }, canSave);
  }

  [Reactive] public Proxy? SelectedProxy { get; set; }
  [Reactive] public ProxyGroup? SelectedGroup { get; set; }
  [Reactive] public Account? SelectedAccount { get; set; }
  [Reactive] public ModuleMetadata? SelectedModule { get; set; }
  public IList<ModuleMetadata> SupportedModules { get; }

  [Reactive] public HarvesterModel? EditingHarvester { get; set; }

  public ReadOnlyObservableCollection<Account> Accounts => _accounts;
  public ReadOnlyObservableCollection<ProxyGroup> ProxyGroups => _proxyGroups;

  public string UrlPathSegment => nameof(HarvesterEditorViewModel);
  public IScreen HostScreen { get; }

  public ReactiveCommand<Unit, Unit> SaveCommand { get; }
}