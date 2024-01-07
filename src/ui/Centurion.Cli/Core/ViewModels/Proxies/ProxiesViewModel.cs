using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Services.Proxies;
using Centurion.Cli.Core.Services.ToastNotifications;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels.Proxies;

public class ProxiesViewModel
  : PageViewModelBase, IRoutableViewModel
{
  private readonly IProxyGroupsRepository _proxyGroupsRepository;
  private readonly IToastNotificationManager _toasts;
  private readonly ProxiesHeaderViewModel _header;
  private readonly ReadOnlyObservableCollection<ProxyGroup> _proxyGroups;
  private readonly IBusyIndicatorFactory _busyIndicatorFactory;

#if DEBUG
  public ProxiesViewModel() : base("Proxies", null)
  {
  }
#endif


  public ProxiesViewModel(IProxyGroupsRepository proxyGroupsRepository, IScreen hostScreen, IMessageBus messageBus,
    IToastNotificationManager toasts, ProxiesHeaderViewModel header, IBusyIndicatorFactory busyIndicatorFactory)
    : base("Proxies", messageBus)
  {
    _proxyGroupsRepository = proxyGroupsRepository;
    _toasts = toasts;
    _header = header;
    _busyIndicatorFactory = busyIndicatorFactory;
    HostScreen = hostScreen;
    var items = proxyGroupsRepository.Items.Connect();
    items
      .Sort(SortExpressionComparer<ProxyGroup>.Ascending(_ => _.Name))
      .Bind(out _proxyGroups)
      .DisposeMany()
      .Subscribe();

    items.ToCollection()
      .Where(i => i.Any())
      .Subscribe(i =>
      {
        if (SelectedProxyGroup is null)
        {
          SelectedProxyGroup = i.First();
        }
      });

    this.WhenAnyValue(_ => _.RawProxies)
      .CombineLatest(this.WhenAnyValue(_ => _.NewGroupName))
      .CombineLatest(this.WhenAnyValue(_ => _.SkipAvailabilityCheck))
      .ObserveOn(RxApp.MainThreadScheduler)
      .Subscribe(_ => ParseProgress = 0);

    var canCreate = this.WhenAnyValue(_ => _.NewGroupName)
      .CombineLatest(this.WhenAnyValue(_ => _.RawProxies),
        (grp, prx) => !string.IsNullOrWhiteSpace(grp) && !string.IsNullOrWhiteSpace(prx));

    var proxyGroupStream = this.WhenAnyValue(_ => _.SelectedProxyGroup);
    var canRemove = proxyGroupStream
      .Select(g => g is not null);

    CreateCommand = ReactiveCommand.CreateFromTask(CreateProxiesAsync, canCreate);
    CreateCommand.IsExecuting.ToPropertyEx(this, _ => _.IsCreating);

    RemoveGroupCommand = ReactiveCommand.CreateFromTask(async ct =>
    {
      if (SelectedProxyGroup is null)
      {
        toasts.Show(ToastContent.Warning("No one group was selected"));
        return;
      }

      await proxyGroupsRepository.RemoveAsync(SelectedProxyGroup, ct);
      SelectedProxyGroup = _proxyGroups.Except(new[] { SelectedProxyGroup }).FirstOrDefault();
      toasts.Show(ToastContent.Success("Group removed successfully"));
    }, canRemove);

    IDisposable? proxyGroupChanged = null;
    // todo: create some kind of list which will do it in his side. The same thing on accounts grid
    proxyGroupStream.Subscribe(g => { proxyGroupChanged = RefreshProxies(proxyGroupChanged, g); });
  }

  private IDisposable? RefreshProxies(IDisposable? proxyGroupChanged, ProxyGroup? g)
  {
    ClearAndDisposeProxies();
    proxyGroupChanged?.Dispose();
    if (g is null)
    {
      return proxyGroupChanged;
    }

    AddAllRows();

    proxyGroupChanged = g.Proxies.ObserveCollectionChanges()
      .Subscribe(_ =>
      {
        ClearAndDisposeProxies();
        AddAllRows();
      });


    void AddAllRows()
    {
      using (ProxyRows.SuspendNotifications())
      {
        ProxyRows.AddRange(
          g.Proxies
            .OrderBy(_ => _.Url)
            .Select(a => new ProxyRowViewModel(a, g, _proxyGroupsRepository, _toasts)));
      }
    }

    return proxyGroupChanged;

    void ClearAndDisposeProxies()
    {
      foreach (var r in ProxyRows)
      {
        r.Dispose();
      }

      ProxyRows.Clear();
    }
  }

  private async Task CreateProxiesAsync(CancellationToken ct)
  {
    if (string.IsNullOrEmpty(RawProxies))
    {
      return;
    }

    using var _ = _busyIndicatorFactory.SwitchToBusyState();
    var validProxies = new List<Proxy>();
    var invalidProxies = new List<string>();
    var tokens = RawProxies.Split(new[] { "\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
    // foreach (var raw in tokens)
    // {
    //   var proxy = await CreateProxyAsync(raw, ct);
    //   if (proxy is not null)
    //   {
    //     validProxies.Add(proxy);
    //   }
    //   else
    //   {
    //     invalidProxies.Add(raw);
    //   }
    // }
    var progressReporter = new BehaviorSubject<double>(0);

    using var progressReporterLiftime = new CompositeDisposable();
    progressReporter.ObserveOn(RxApp.MainThreadScheduler)
      .Subscribe(p => ParseProgress = p)
      .DisposeWith(progressReporterLiftime);

    var gates = new SemaphoreSlim(1, 1);
    var proxyCreateTasks = tokens.Select(raw => Task.Run(async () =>
    {
      var proxy = await CreateProxyAsync(raw, ct);
      try
      {
        await gates.WaitAsync(CancellationToken.None);
        if (proxy is not null)
        {
          validProxies.Add(proxy);
        }
        else
        {
          invalidProxies.Add(raw);
        }

        var nextProgress = (validProxies.Count + invalidProxies.Count) / (double)tokens.Length;
        progressReporter.OnNext(nextProgress * 100);
      }
      finally
      {
        gates.Release();
      }
    }, ct));

    await Task.WhenAll(proxyCreateTasks);

    if (invalidProxies.Any())
    {
      _toasts.Show(ToastContent.Error("Some of proxies are malformed or unavailable. They were skipped.",
        "Can't save all proxies"));

      // _toasts.Show(ToastContent.Error(
      //   "Some of proxies are malformed. Please check them and try again", "Can't save all proxies"));
      RawProxies = string.Join(Environment.NewLine, invalidProxies);
    }
    else
    {
      _toasts.Show(ToastContent.Success("Proxies are saved."));
      RawProxies = null;
    }

    if (!validProxies.Any())
    {
      _toasts.Show(ToastContent.Error($"No valid proxies. They maybe are malformed or unavailable"));
      return;
    }


    var group = new ProxyGroup(NewGroupName!, validProxies);
    await _proxyGroupsRepository.SaveAsync(group, ct);
    SelectedProxyGroup = group;
    NewGroupName = null;
    SkipAvailabilityCheck = false;
  }

  private async Task<Proxy?> CreateProxyAsync(string raw, CancellationToken ct)
  {
    if (Proxy.TryParse(raw, out var proxy) && (SkipAvailabilityCheck || await IsProxyAvailableAsync(proxy, ct)))
    {
      // validProxies.Add(proxy!);
      // if (!SkipAvailabilityCheck)
      // {
      //   await Task.Delay(TimeSpan.FromMilliseconds(25), ct);
      // }
      return proxy;
    }

    return null;
  }

  private async Task<bool> IsProxyAvailableAsync(Proxy? proxy, CancellationToken ct)
  {
    if (proxy is null)
    {
      return false;
    }

    var client = new HttpClient(new HttpClientHandler
    {
      Proxy = proxy.ToWebProxy(),
      UseProxy = true,
    })
    {
      Timeout = TimeSpan.FromSeconds(10)
    };

    var message = new HttpRequestMessage(HttpMethod.Get, "https://taskmanager-api.centurion.gg/heartbeat");
    try
    {
      var r = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, ct);
      return r.IsSuccessStatusCode;
    }
    catch
    {
      return false;
    }
  }

  protected override ViewModelBase GetHeaderContent() => _header;

  [Reactive] public ProxyRowViewModel? SelectedProxyRow { get; set; }
  public ObservableCollectionExtended<ProxyRowViewModel> ProxyRows { get; } = new();


  public ProxiesHeaderViewModel Header => _header;
  public ReadOnlyObservableCollection<ProxyGroup> ProxyGroups => _proxyGroups;
  [Reactive] public ProxyGroup? SelectedProxyGroup { get; set; }
  [Reactive] public string? NewGroupName { get; set; }
  [Reactive] public string? RawProxies { get; set; }
  [Reactive] public bool SkipAvailabilityCheck { get; set; }
  [Reactive] public double ParseProgress { get; set; }
  public string UrlPathSegment => nameof(ProxiesViewModel);
  public IScreen HostScreen { get; }
  public bool IsCreating { [ObservableAsProperty] get; }

  public ReactiveCommand<Unit, Unit> CreateCommand { get; }
  public ReactiveCommand<Unit, Unit> RemoveGroupCommand { get; }
}