using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Centurion.Cli.Core.Composition;
using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Domain.Accounts;
using Centurion.Cli.Core.Services.Accounts;
using Centurion.Cli.Core.Services.Harvesters;
using Centurion.Cli.Core.Services.Proxies;
using Centurion.Cli.Core.Services.ToastNotifications;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace Centurion.Cli.Core.ViewModels.Harvesters;

[TransientViewModel]
public class HarvesterItemViewModel : ViewModelBase
{
#if DEBUG
  public HarvesterItemViewModel()
  {
  }
#endif


  public HarvesterItemViewModel(IAccountsRepository accounts, IProxyGroupsRepository proxyGroups,
    IHarvesterFactory harvesterFactory, IHarvesterRegistry harvesterRegistry, IToastNotificationManager toasts,
    IHarvestersRepository harvestersRepo)
  {
    this.WhenAnyValue(_ => _.Harvester)
      .WhereNotNull()
      .Subscribe(h =>
      {
        Account = accounts.Items.Items.FirstOrDefault(_ => _.Id == h.AccountId);
        Proxy = proxyGroups.Items.Items.SelectMany(_ => _.Proxies).FirstOrDefault(_ => _.Id == h.ProxyId);
      })
      .DisposeWith(Disposable);

    var isIdle = this.WhenAnyValue(_ => _.Status).Select(it => it is HarvesterStatus.Idle);
    var hasProxy = this.WhenAnyValue(_ => _.Proxy).Select(it => it is not null);
    var hasAccount = this.WhenAnyValue(_ => _.Account).Select(it => it is not null);

    var canStartHarvester = hasAccount.CombineLatest(hasProxy, isIdle, (acc, proxy, idle) => acc && proxy && idle)
      .ObserveOn(RxApp.MainThreadScheduler);

    StartCommand = ReactiveCommand.CreateFromTask(async ct =>
    {
      if (Status is not HarvesterStatus.Idle)
      {
        toasts.Show(ToastContent.Warning("Can't start harvester. Its not in idle state"));
        return;
      }

      try
      {
        Status = HarvesterStatus.Initializing;

        var lifetime = new CompositeDisposable();
        var tcs = new TaskCompletionSource();
        IHarvester harvester = default!;
        _ = Task.Factory.StartNew(() =>
        {
          harvester = harvesterFactory.Create();
          harvester.Terminated += HarvesterOnTerminated;
          harvester.Start(new InitializedHarvesterModel(Proxy!, Account!, Harvester!), ct).AsTask().GetAwaiter().GetResult();
          tcs.SetResult();
        }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        await tcs.Task;
        harvester.TokensHarvested
          .ObserveOn(RxApp.MainThreadScheduler)
          .ToPropertyEx(this, _ => _.TokensHarvested)
          .DisposeWith(lifetime);
        var registration = harvesterRegistry.Register(harvester);
        lifetime.Add(registration);
        Status = HarvesterStatus.Running;

        void HarvesterOnTerminated(object? sender, EventArgs e)
        {
          lifetime.Dispose();
          harvester.Terminated -= HarvesterOnTerminated;
          Status = HarvesterStatus.Idle;
        }
      }
      catch (Exception /* ignore */)
      {
      }
    }, canStartHarvester);

    RemoveCommand = ReactiveCommand.CreateFromTask(async ct => { await harvestersRepo.RemoveAsync(Harvester!, ct); });
  }

  public static HarvesterItemViewModel Create(HarvesterModel harvester, IReadonlyDependencyResolver resolver)
  {
    var vm = resolver.GetService<HarvesterItemViewModel>()!;
    vm.Harvester = harvester;

    return vm;
  }

  public int TokensHarvested { [ObservableAsProperty] get; }
  [Reactive] public HarvesterStatus Status { get; private set; }

  [Reactive] public HarvesterModel Harvester { get; private set; }
  [Reactive] public Account? Account { get; private set; }
  [Reactive] public Proxy? Proxy { get; private set; }

  public ReactiveCommand<Unit, Unit> StartCommand { get; }
  public ReactiveCommand<Unit, Unit> RemoveCommand { get; }
}

public enum HarvesterStatus
{
  Idle,
  Initializing,
  Running
}