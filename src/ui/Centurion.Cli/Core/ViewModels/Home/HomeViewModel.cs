using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using AutoMapper;
using Avalonia.Controls.Mixins;
using Centurion.Cli.Core.Domain.Tasks;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Services.Analytics;
using Centurion.Cli.Core.ViewModels.Tasks;
using Centurion.Contracts.Analytics;
using Centurion.Contracts.Checkout.Integration;
using Centurion.Contracts.TaskManager;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels.Home;

public class HomeViewModel : ViewModelBase, IRoutableViewModel
{
  private readonly TaskEditorViewModel _taskEditor;
  private readonly ReadOnlyObservableCollection<CheckoutItemRowViewModel> _checkouts;
  private readonly ReadOnlyObservableCollection<PresetItemViewModel> _presets;

#if DEBUG
#pragma warning disable CS8618
  public HomeViewModel()
#pragma warning restore CS8618
  {
  }
#endif

  public HomeViewModel(IScreen hostScreen, IAnalyticsService analyticsService, IMessageBus messageBus,
    IPresetsService presetsService, IMapper mapper, TaskEditorViewModel taskEditor, TasksViewModel tasks)
  {
    _taskEditor = taskEditor;
    HostScreen = hostScreen;
    Tasks = tasks;

    analyticsService.AnalyticsSummary
      .ToPropertyEx(this, _ => _.Summary)
      .DisposeWith(Disposable);

    this.WhenAnyValue(_ => _.Summary)
      .Select(_ => _.Entries.Select(mapper.Map<CheckoutEntryData>).ToList())
      .Subscribe(e => Entries = e)
      .DisposeWith(Disposable);

    analyticsService.Checkouts
      .Connect()
      .Transform(i => new CheckoutItemRowViewModel(i))
      .Bind(out _checkouts)
      .Subscribe()
      .DisposeWith(Disposable);

    presetsService.Presets
      .Connect()
      .Bind(out _presets)
      .Subscribe()
      .DisposeWith(Disposable);

    FetchPageCommand = ReactiveCommand.CreateFromTask<int>(async (ix, ct) =>
    {
      Page = await analyticsService.FetchPage(ix, ct);
    });

    messageBus.Listen<ProductCheckedOut>()
      .Throttle(TimeSpan.FromMilliseconds(500))
      .Do(_ =>
        Task.Run(async () =>
          {
            analyticsService.ResetCache();
            await analyticsService.InitializeAsync();
            await FetchPageCommand.Execute(0).FirstOrDefaultAsync();
          })
          .ToObservable()
      )
      .Subscribe();


    OpenTaskEditorForPresetCommand = ReactiveCommand.Create<PresetData>(preset =>
    {
      var task = mapper.Map<CheckoutTaskModel>(preset);
      taskEditor.EditTask(task);
    });
  }

  public TaskEditorViewModel Editor => _taskEditor;
  public TasksViewModel Tasks { get; }
  [Reactive] public CheckoutInfoPagedList Page { get; set; }

  public AnalyticsSummary Summary { [ObservableAsProperty] get; }

  [Reactive] public IList<CheckoutEntryData> Entries { get; private set; } = Array.Empty<CheckoutEntryData>();

  public string UrlPathSegment => nameof(HomeViewModel);
  public IScreen HostScreen { get; }

  public ReadOnlyObservableCollection<CheckoutItemRowViewModel> Checkouts => _checkouts;
  public ReadOnlyObservableCollection<PresetItemViewModel> Presets => _presets;

  public ReactiveCommand<int, Unit> FetchPageCommand { get; private set; }
  public ReactiveCommand<PresetData, Unit> OpenTaskEditorForPresetCommand { get; private set; }
}

public class CheckoutEntryData
{
  public DateTime Date { get; set; }
  public decimal TotalPrice { get; set; }
  public uint Count { get; set; }
}