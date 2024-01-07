using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Centurion.Cli.Core.Domain.Profiles;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Services.Profiles;
using Centurion.Cli.Core.Services.ToastNotifications;
using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels.Profiles;

public class ProfilesViewModel : PageViewModelBase, IRoutableViewModel
{
  private readonly IProfilesRepository _profilesRepository;
  private readonly ProfilesHeaderSearchViewModel _search;
  private readonly ProfileEditorViewModel _profileEditor;
  private readonly IClipboardService _clipboardService;
  private readonly IToastNotificationManager _toasts;
  private readonly ReadOnlyObservableCollection<ProfileGroupModel> _profileGroups;

  private readonly IDialogService _dialogService;

#if DEBUG
  public ProfilesViewModel()
    : base("Profiles", null)
  {
  }

#endif

  public ProfilesViewModel(IMessageBus messageBus, IScreen hostScreen, IProfilesRepository profilesRepository,
    ProfilesHeaderSearchViewModel search, ProfileEditorViewModel profileEditor, IClipboardService clipboardService,
    IToastNotificationManager toasts, IDialogService dialogService)
    : base("Profiles", messageBus)
  {
    _profilesRepository = profilesRepository;
    _search = search;
    _profileEditor = profileEditor;
    _clipboardService = clipboardService;
    _toasts = toasts;
    _dialogService = dialogService;
    HostScreen = hostScreen;

    profilesRepository.Items.Connect()
      .SortBy(_ => _.Name)
      .Bind(out _profileGroups)
      .Subscribe();

    // var filter = search.WhenAnyValue(_ => _.SearchTerm)
    //   .Throttle(TimeSpan.FromMilliseconds(300))
    //   .ObserveOn(RxApp.MainThreadScheduler)
    //   .Select(BuildSearch);

    var selectedGroup = this.WhenAnyValue(_ => _.SelectedGroup)
      .Replay(1)
      .RefCount();

    var canWriteProfile = selectedGroup.Select(it => it is not null);
    CreateProfileCommand = ReactiveCommand.Create(OpenCreateProfileEditor, canWriteProfile);

    var canCreateGroup = this.WhenAnyValue(_ => _.NewGroupName)
      .Select(g => !string.IsNullOrWhiteSpace(g));
    CreateProfileGroupCommand = ReactiveCommand.CreateFromTask(CreateProfileGroup, canCreateGroup);
    EditCommand = ReactiveCommand.Create<ProfileModel>(OpenProfileEditor);
    CopyToClipboardCommand = ReactiveCommand.CreateFromTask<ProfileModel>(CopyToClipboard);
    RemoveCommand = ReactiveCommand.CreateFromTask<ProfileModel>(RemoveProfile);
    ImportIntoGroupCommand = ReactiveCommand.CreateFromTask(ImportIntoSelectedGroup);
    RemoveSelectedGroupCommand = ReactiveCommand.CreateFromTask(RemoveGroup, canWriteProfile);

    IDisposable? groupChanged = null;
    // todo: create some kind of list which will do it in his side. The same thing on accounts grid
    selectedGroup.Subscribe(g => { groupChanged = RefreshProfiles(groupChanged, g); });
  }

  private void OpenCreateProfileEditor()
  {
    _profileEditor.EditProfile(SelectedGroup!, new ProfileModel());
    // HostScreen.Router.Navigate.Execute(_profileEditor).Subscribe();
  }

  private async Task CreateProfileGroup(CancellationToken ct)
  {
    var newGroup = new ProfileGroupModel
    {
      Name = NewGroupName!
    };

    await _profilesRepository.SaveAsync(newGroup, ct);
    NewGroupName = null;
  }

  private void OpenProfileEditor(ProfileModel profile)
  {
    _profileEditor.EditProfile(SelectedGroup!, profile);
    // HostScreen.Router.Navigate.Execute(_profileEditor).Subscribe();
  }

  private async Task RemoveGroup(CancellationToken ct)
  {
    var toRemove = SelectedGroup!;
    await _profilesRepository.RemoveAsync(toRemove, ct);
    _toasts.Show(ToastContent.Success($"Group {toRemove.Name} removed"));
  }

  private async Task RemoveProfile(ProfileModel profile, CancellationToken ct)
  {
    SelectedGroup!.Profiles.Remove(profile);
    await _profilesRepository.SaveSilentlyAsync(SelectedGroup, ct);
    _toasts.Show(ToastContent.Success($"Profile '{profile.Name}' removed."));
  }

  private async Task CopyToClipboard(ProfileModel profile, CancellationToken ct)
  {
    var json = JsonConvert.SerializeObject(profile, new JsonSerializerSettings
    {
      Formatting = Formatting.Indented,
      ContractResolver = new CamelCasePropertyNamesContractResolver()
    });

    await _clipboardService.SetTextAsync(json, ct);

    _toasts.Show(ToastContent.Success($"Profile '{profile.Name}' copied to clipboard"));
  }

  private async Task ImportIntoSelectedGroup(CancellationToken ct)
  {
    var importFilePath =
      await _dialogService.PickOpenFileAsync("Please select file to import data", ".json");
    if (string.IsNullOrEmpty(importFilePath))
    {
      return;
    }

    var ext = Path.GetExtension(importFilePath).ToLowerInvariant();

    bool isSuccessful;
    await using var file = File.OpenRead(importFilePath);
    switch (ext)
    {
      case ".json":
        isSuccessful = await _search.ImportExportService.ImportFromJsonIntoGroupAsync(file, SelectedGroup!, ct);
        break;
      default:
        _toasts.Show(ToastContent.Error($"Selected file has invalid format: '{ext}'. Supported only JSON"));
        return;
    }

    if (!isSuccessful)
    {
      _toasts.Show(ToastContent.Warning("Selected file is empty"));
      return;
    }

    _toasts.Show(ToastContent.Success($"All data were imported."));
  }

  protected override ViewModelBase GetHeaderContent() => _search;

  private IDisposable? RefreshProfiles(IDisposable? changed, ProfileGroupModel? g)
  {
    using var __ = Profiles.SuspendNotifications();
    ClearAndDisposeProfiles();
    changed?.Dispose();
    if (g is null)
    {
      return changed;
    }

    AddAllRows();

    changed = g.Profiles.ObserveCollectionChanges()
      .Subscribe(_ =>
      {
        ClearAndDisposeProfiles();
        AddAllRows();
      });


    void AddAllRows()
    {
      Profiles.AddRange(
        g.Profiles
          .OrderBy(_ => _.Name)
          .Select(p => new ProfileRowViewModel(p)));
    }

    void ClearAndDisposeProfiles()
    {
      foreach (var p in Profiles)
      {
        p.Dispose();
      }

      Profiles.Clear();
    }

    return changed;
  }


  /*private Func<ProfileRowViewModel, bool> BuildSearch(string? searchTerm)
  {
    if (string.IsNullOrWhiteSpace(searchTerm))
    {
      return _ => true;
    }

    return _ => _.Profile.Name.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase);
  }*/

  public ProfileEditorViewModel Editor => _profileEditor;
  public ProfilesHeaderSearchViewModel Header => _search;
  public ObservableCollectionExtended<ProfileRowViewModel> Profiles { get; } = new();
  public ReadOnlyObservableCollection<ProfileGroupModel> ProfileGroups => _profileGroups;

  public ReactiveCommand<Unit, Unit> CreateProfileGroupCommand { get; }
  public ReactiveCommand<Unit, Unit> CreateProfileCommand { get; }
  public ReactiveCommand<Unit, Unit> ImportIntoGroupCommand { get; }
  public ReactiveCommand<Unit, Unit> RemoveSelectedGroupCommand { get; }

  public ReactiveCommand<ProfileModel, Unit> EditCommand { get; }
  public ReactiveCommand<ProfileModel, Unit> CopyToClipboardCommand { get; }
  public ReactiveCommand<ProfileModel, Unit> RemoveCommand { get; }

  [Reactive] public string? NewGroupName { get; set; }
  [Reactive] public ProfileGroupModel? SelectedGroup { get; set; }
  public string UrlPathSegment => nameof(ProfilesViewModel);
  public IScreen HostScreen { get; }
}