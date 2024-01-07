using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using AutoMapper;
using Centurion.Cli.Core.Composition;
using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Domain.Profiles;
using Centurion.Cli.Core.Domain.Tasks;
using Centurion.Cli.Core.Modules;
using Centurion.Cli.Core.Services.Modules;
using Centurion.Cli.Core.Services.Profiles;
using Centurion.Cli.Core.Services.Proxies;
using Centurion.Cli.Core.Services.ToastNotifications;
using Centurion.Cli.Core.Validators;
using Centurion.Contracts;
using DynamicData;
using Google.Protobuf;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels.Tasks;

[TransientViewModel]
public class TaskEditorViewModel : ViewModelBase, IRoutableViewModel
{
  private readonly IMapper _mapper;
  private readonly TaskEditorValidator _validator;
  private readonly IToastNotificationManager _toasts;
  private CheckoutTaskModel? _sourceTask;
  private readonly IModuleReflector _moduleReflector;

  private readonly ReadOnlyObservableCollection<ProfileModel> _profiles;
  private readonly ReadOnlyObservableCollection<ProfileGroupModel> _profileGroups;

  private readonly ReadOnlyObservableCollection<ProxyGroup> _proxies;
  // private readonly ReadOnlyObservableCollection<SessionModel> _sessions;

#if DEBUG
  public TaskEditorViewModel()
  {
  }
#endif

  public TaskEditorViewModel(IScreen hostScreen, IMapper mapper, IProfilesRepository profiles,
    IProxyGroupsRepository proxies, IModuleMetadataProvider moduleMetadataProvider /*, ISessionRepository sessions*/,
    TaskEditorValidator validator, IToastNotificationManager toasts, IModuleReflector moduleReflector)
  {
    _mapper = mapper;
    _validator = validator;
    _toasts = toasts;
    _moduleReflector = moduleReflector;
    HostScreen = hostScreen;
    SupportedModules = moduleMetadataProvider.SupportedModules;

    profiles.Items.Connect()
      .TransformMany(_ => _.Profiles, _ => _.Id)
      .Bind(out _profiles)
      .Subscribe()
      .DisposeWith(Disposable);

    profiles.Items.Connect()
      .Bind(out _profileGroups)
      .Subscribe()
      .DisposeWith(Disposable);
    //
    // sessions.Items.Connect()
    //   .Bind(out _sessions)
    //   .Subscribe()
    //   .DisposeWith(Disposable);

    proxies.Items.Connect()
      .Bind(out _proxies)
      .Subscribe()
      .DisposeWith(Disposable);

    this.WhenAnyValue(_ => _.SelectedModuleMetadata)
      .DistinctUntilChanged(_ => _?.Module)
      .Subscribe(m =>
      {
        Config = m is not null ? _moduleReflector.CreateEmpty(m.Config) : null;
        SupportedModes = m?.Modes.ToArray() ?? Array.Empty<CheckoutModeMetadata>();
        SelectedModeMetadata = null;
      })
      .DisposeWith(Disposable);

    this.WhenAnyValue(_ => _.SelectedModeMetadata)
      .DistinctUntilChanged()
      .Subscribe(mode =>
      {
        ModeConfig = mode is null ? null : _moduleReflector.GetOrSet(Config!, mode.Config);
      })
      .DisposeWith(Disposable);

    SaveChangesCommand = ReactiveCommand.CreateFromTask(SaveChanges);

    CancelCommand = ReactiveCommand.Create(() => EditTask(_sourceTask));
    ClearCheckoutProxy = ReactiveCommand.Create(() => { CheckoutProxyPool = null; });
    ClearMonitorProxy = ReactiveCommand.Create(() => { MonitorProxyPool = null; });
  }

  private async Task<CheckoutTaskModel?> SaveChanges(CancellationToken ct)
  {
    var validationResult = await _validator.ValidateAsync(this, ct);
    if (!validationResult.IsValid)
    {
      var message = string.Join("\n", validationResult.Errors.Select(_ => _.ErrorMessage));
      _toasts.Show(ToastContent.Error(message));
      return null;
    }

    EditingTask.Module = SelectedModuleMetadata!.Module;

    EditingTask.ProfileIds = UseRandomProfile
      ? ProfileGroup!.Profiles.Select(_ => _.Id).ToHashSet()
      : new HashSet<Guid> { Profile!.Id };

    // EditingTask.SessionId = Session?.Id;
    EditingTask.CheckoutProxyPoolId = CheckoutProxyPool?.Id;
    EditingTask.MonitorProxyPoolId = MonitorProxyPool?.Id;

    if (Config != null)
    {
      EditingTask.Config = Config.ToByteArray();
    }

    _mapper.Map(EditingTask, _sourceTask);

    return _sourceTask!;
  }

  public void EditTask(CheckoutTaskModel? toEdit)
  {
    Reset();
    SaveProgress = 0;
    IsBusy = false;
    IsNew = toEdit is null || toEdit.Id == default;
    _sourceTask = toEdit ?? new CheckoutTaskModel();
    _mapper.Map(_sourceTask, EditingTask);
    TaskCountToCreate = 1;
    SelectedModuleMetadata = SupportedModules.FirstOrDefault(_ => _.Module == EditingTask.Module);
    if (SelectedModuleMetadata is not null)
    {
      Config = _moduleReflector.Create(SelectedModuleMetadata.Config, _sourceTask.Config);
      SelectedModeMetadata = _moduleReflector.GetSelectedModeOrDefault(Config!, SelectedModuleMetadata)
        ?? SelectedModuleMetadata.Modes[0];
    }

    CheckoutProxyPool = _proxies.FirstOrDefault(_ => _.Id == EditingTask.CheckoutProxyPoolId);
    MonitorProxyPool = _proxies.FirstOrDefault(_ => _.Id == EditingTask.MonitorProxyPoolId);

    UseRandomProfile = EditingTask.ProfileIds.Count > 1;
    if (UseRandomProfile)
    {
      ProfileGroup = _profileGroups.FirstOrDefault(g => g.Profiles.Any(p => EditingTask.ProfileIds.Contains(p.Id)));
      Profile = null;
    }
    else
    {
      Profile = _profiles.FirstOrDefault(_ => _.Id == EditingTask.ProfileIds.FirstOrDefault());
      ProfileGroup = null;
    }
  }

  public IDisposable SwitchToBusyState()
  {
    IsBusy = true;
    SaveProgress = 0;
    return System.Reactive.Disposables.Disposable.Create(() => { IsBusy = false; });
  }

  private void Reset()
  {
    SelectedModeMetadata = null;
    SelectedModuleMetadata = null;
    CheckoutProxyPool = null;
    MonitorProxyPool = null;
    Profile = null;
    ProfileGroup = null;
  }

  public void UpdateProgress(int done, int total)
  {
    SaveProgress = done / (double)total * 100;
  }

  [Reactive] public bool IsNew { get; private set; }
  [Reactive] public bool IsBusy { get; private set; }
  [Reactive] public double SaveProgress { get; private set; }

  [Reactive] public int TaskCountToCreate { get; set; }

  public ReadOnlyObservableCollection<ProfileModel> Profiles => _profiles;
  public ReadOnlyObservableCollection<ProfileGroupModel> ProfileGroups => _profileGroups;

  public ReadOnlyObservableCollection<ProxyGroup> Proxies => _proxies;
  // public ReadOnlyObservableCollection<SessionModel> Sessions => _sessions;

  public CheckoutTaskModel EditingTask { get; } = new();

  [Reactive] public ModuleMetadata? SelectedModuleMetadata { get; set; }
  [Reactive] public CheckoutModeMetadata? SelectedModeMetadata { get; set; }

  public IReadOnlyList<ModuleMetadata> SupportedModules { get; }
  [Reactive] public IReadOnlyList<CheckoutModeMetadata> SupportedModes { get; private set; }

  [Reactive] public ProxyGroup? CheckoutProxyPool { get; set; }
  public ReactiveCommand<Unit, Unit> ClearCheckoutProxy { get; }

  [Reactive] public ProxyGroup? MonitorProxyPool { get; set; }
  public ReactiveCommand<Unit, Unit> ClearMonitorProxy { get; }

  [Reactive] public ProfileModel? Profile { get; set; }
  [Reactive] public ProfileGroupModel? ProfileGroup { get; set; }

  [Reactive] public bool UseRandomProfile { get; set; }
  // [Reactive] public SessionModel? Session { get; set; }

  public string UrlPathSegment => nameof(TaskEditorViewModel);
  public IScreen HostScreen { get; }

  [Reactive] public IMessage? Config { get; set; }
  [Reactive] public IMessage? ModeConfig { get; set; }
  [Reactive] public IMessage? MonitorConfig { get; set; }

  public ReactiveCommand<Unit, CheckoutTaskModel?> SaveChangesCommand { get; }
  public ReactiveCommand<Unit, Unit> CancelCommand { get; }
}