using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using AutoMapper;
using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Domain.Accounts;
using Centurion.Cli.Core.Services.Accounts;
using Centurion.Cli.Core.Services.Sessions;
using Centurion.Cli.Core.Services.ToastNotifications;
using Centurion.Contracts;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels.Sessions;

public class SessionEditorViewModel : ViewModelBase, IRoutableViewModel
{
  private readonly IMapper _mapper;
  private SessionModel? _sourceTask;
  private readonly ReadOnlyObservableCollection<Account> _accounts;
  private readonly IToastNotificationManager _toasts;
  private readonly ISessionConfigurationQueue _sessionConfigurationQueue;

  public SessionEditorViewModel(IScreen hostScreen, IAccountsRepository accountsRepository, IMapper mapper,
    IToastNotificationManager toasts, ISessionConfigurationQueue sessionConfigurationQueue)
  {
    _mapper = mapper;
    _toasts = toasts;
    _sessionConfigurationQueue = sessionConfigurationQueue;
    HostScreen = hostScreen;

    accountsRepository.Items.Connect()
      .Bind(out _accounts)
      .Subscribe()
      .DisposeWith(Disposable);

    Statuses = Enum.GetValues<SessionStatus>()
      .Select(s => new KeyValuePair<string, SessionStatus>(s.ToString(), s))
      .ToArray();

    var dataLifetime = new CompositeDisposable();

    this.WhenAnyValue(_ => _.Data)
      .Subscribe(data =>
      {
        dataLifetime.Clear();
        if (data is null)
        {
          return;
        }

        data.WhenAnyValue(_ => _.AccountId)
          .Select(accountId => accountsRepository.Items.WatchValue(accountId))
          .Switch()
          .DistinctUntilChanged()
          .Subscribe(a => SelectedAccount = a)
          .DisposeWith(dataLifetime);


        this.WhenAnyValue(_ => _.SelectedAccount)
          .Select(_ => (_?.Id).GetValueOrDefault())
          .DistinctUntilChanged()
          .Subscribe(id => data.AccountId = id)
          .DisposeWith(dataLifetime);

        _sessionConfigurationQueue.SessionProcessed
          .Where(s => s.Id == data.Id)
          .Subscribe(_ => DetectAutoConfigPending())
          .DisposeWith(dataLifetime);
      })
      .DisposeWith(Disposable);

    SaveChangesCommand = ReactiveCommand.Create(() =>
    {
      Data!.AccountId = SelectedAccount!.Id;
      Data.Module = Module.Amazon;
      DetectAutoConfigPending();
      if (!IsAutoConfigPending)
      {
        Data.Status = SessionStatus.Ready;
      }

      _mapper.Map(Data, _sourceTask);
      Data = null;
      return _sourceTask!;
    });

    CancelCommand = ReactiveCommand.Create(() =>
    {
      _sourceTask = null;
      Data = null;
    });

    var canAutoconfigure = this.WhenAnyValue(_ => _.SelectedAccount)
      .Select(a => a is not null);

    AutoconfigureCommand = ReactiveCommand.CreateFromTask(Autoconfigure, canAutoconfigure);
  }

  private async Task Autoconfigure()
  {
    await _sessionConfigurationQueue.Enqueue(Data!, SelectedAccount!);
    _toasts.Show(ToastContent.Success("Configuration queued"));
    DetectAutoConfigPending();
    await SaveChangesCommand.Execute().FirstOrDefaultAsync();
    // using var _ = _busyIndicatorFactory.SwitchToBusyState();
    // try
    // {
    //   var cookies = await _autoconfigurator.Configure(SelectedAccount!, ct);
    //   Data.Cookies = new HashSet<string>(cookies);
    //   _toasts.Show(ToastContent.Success("Cookies collected"));
    // }
    // catch
    // {
    //   _toasts.Show(ToastContent.Error("Failed to enable one-click"));
    // }
  }

  private void DetectAutoConfigPending()
  {
    if (Data is null)
    {
      return;
    }

    IsAutoConfigPending = _sessionConfigurationQueue.IsQueued(Data);
  }

  [Reactive] public bool IsAutoConfigPending { get; private set; }
  [Reactive] public Account? SelectedAccount { get; set; }
  public ReadOnlyObservableCollection<Account> Accounts => _accounts;
  [Reactive] public SessionModel? Data { get; private set; }

  public void EditSession(SessionModel? toEdit)
  {
    _sourceTask = toEdit ?? new SessionModel();
    Data = new SessionModel();
    _mapper.Map(toEdit, Data);
    DetectAutoConfigPending();
  }

  public IReadOnlyList<KeyValuePair<string, SessionStatus>> Statuses { get; }
  public string UrlPathSegment => nameof(SessionEditorViewModel);
  public IScreen HostScreen { get; }

  public ReactiveCommand<Unit, Unit> AutoconfigureCommand { get; }
  public ReactiveCommand<Unit, SessionModel> SaveChangesCommand { get; }
  public ReactiveCommand<Unit, Unit> CancelCommand { get; }
}