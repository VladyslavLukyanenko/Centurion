using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls.Mixins;
using Centurion.Cli.Core.Config;
using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Services.Harvesters;
using Centurion.Cli.Core.Services.ToastNotifications;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace Centurion.Cli.Core.ViewModels.Harvesters;

public class HarvestersViewModel : PageViewModelBase, IRoutableViewModel
{
  private readonly ReadOnlyObservableCollection<HarvesterItemViewModel> _harvesters;
  private readonly HarvesterConfig _config = default!;

#if DEBUG
  public HarvestersViewModel()
    : base("Harvesters", default!)
  {
  }
#endif


  public HarvestersViewModel(IScreen hostScreen, IHarvestersRepository harvestersRepository,
    IReadonlyDependencyResolver resolver, IMessageBus messageBus, HarvesterEditorViewModel editor,
    IToastNotificationManager toasts, HarvesterConfig config)
    : base("Harvesters", messageBus)
  {
    HostScreen = hostScreen;
    Editor = editor;
    _config = config;

    var harvesters = harvestersRepository.Items.Connect()
      .Transform(h => HarvesterItemViewModel.Create(h, resolver))
      .Replay(1)
      .RefCount();

    harvesters
      .SortBy(_ => _.Account?.Email!)
      .Bind(out _harvesters)
      .Subscribe()
      .DisposeWith(Disposable);

    harvesters
      .AutoRefresh(_ => _.Status)
      .Filter(_ => _.Status == HarvesterStatus.Running)
      .ToCollection()
      .Select(_ => _.Count)
      .ToPropertyEx(this, _ => _.RunningCount)
      .DisposeWith(Disposable);

    harvesters
      .AutoRefresh(_ => _.TokensHarvested)
      .ToCollection()
      .Select(_ => _.Sum(_ => _.TokensHarvested))
      .ToPropertyEx(this, _ => _.TokensHarvestedTotal)
      .DisposeWith(Disposable);

    CreateNewCommand = ReactiveCommand.Create(() => { Editor.EditingHarvester = new HarvesterModel(); });
    RemoveCachesCommand = ReactiveCommand.Create(() =>
    {
      if (_harvesters.Any(_ => _.Status != HarvesterStatus.Idle))
      {
        toasts.Show(ToastContent.Error("Can't remove caches because some of the harvesters are running"));
        return;
      }

      if (Directory.Exists(_config.ChromiumBaseUserDir))
      {
        Directory.Delete(_config.ChromiumBaseUserDir, true);
      }
    });
  }

  public ReadOnlyObservableCollection<HarvesterItemViewModel> Harvesters => _harvesters;
  public string UrlPathSegment => nameof(HarvestersViewModel);
  public IScreen HostScreen { get; }
  public HarvesterEditorViewModel Editor { get; }

  public int RunningCount { [ObservableAsProperty] get; }
  public int TokensHarvestedTotal { [ObservableAsProperty] get; }

  public ReactiveCommand<Unit, Unit> CreateNewCommand { get; }
  public ReactiveCommand<Unit, Unit> RemoveCachesCommand { get; }
}