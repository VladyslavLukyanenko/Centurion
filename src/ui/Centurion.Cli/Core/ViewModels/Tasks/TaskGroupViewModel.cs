using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using AutoMapper;
using Avalonia.Collections;
using Centurion.Cli.Core.Composition;
using Centurion.Cli.Core.Domain.Tasks;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Services.Tasks;
using Centurion.Cli.Core.Services.ToastNotifications;
using Centurion.Contracts;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace Centurion.Cli.Core.ViewModels.Tasks;

[TransientViewModel]
public class TaskGroupViewModel : ViewModelBase
{
  private readonly ITasksService _tasksService;
  private readonly TaskEditorViewModel _taskEditor;
  private readonly IToastNotificationManager _toasts;
  private readonly IBusyIndicatorFactory _busyIndicatorFactory;
  private readonly ITaskViewService _taskViewService;
  private readonly IMapper _mapper;

  private ReadOnlyObservableCollection<TaskRowViewModel> _tasks = null!;

#if DEBUG
  public TaskGroupViewModel()
  {
  }
#endif

  public TaskGroupViewModel(ITasksService tasksService, TaskEditorViewModel taskEditor,
    IToastNotificationManager toasts, IBusyIndicatorFactory busyIndicatorFactory, ITaskViewService taskViewService,
    IMapper mapper)
  {
    _tasksService = tasksService;
    _taskEditor = taskEditor;
    _toasts = toasts;
    _busyIndicatorFactory = busyIndicatorFactory;
    _taskViewService = taskViewService;
    _mapper = mapper;
    //
    // statusRegistry.Statuses.Connect()
    //   .Bind(out _statuses)
    //   .Subscribe()
    //   .DisposeWith(Disposable);


    ToggleSelectAllTasksCommand = ReactiveCommand.Create(() =>
    {
      if (IsSelectedAllTasks == true)
      {
        SelectedRows.Clear();
      }
      else
      {
        using var _ = SelectedRows.SuspendNotifications();
        SelectedRows.Clear();
        SelectedRows.AddRange(_tasks);
      }
    });

    ToggleSelectedTaskCommand = ReactiveCommand.Create<TaskRowViewModel>(t =>
    {
      if (SelectedRows.Contains(t))
      {
        SelectedRows.Remove(t);
      }
      else
      {
        SelectedRows.Add(t);
      }
    });

    CreateTaskCommand = ReactiveCommand.Create(OpenCreateTaskEditor);
    EditCommand = ReactiveCommand.Create(OpenEditTaskEditor);
    RemoveCommand = ReactiveCommand.CreateFromTask(RemoveTask);
    RemoveAllCommand = ReactiveCommand.CreateFromTask(RemoveAllTasks);

    StartCommand = ReactiveCommand.CreateFromTask(StartTask);
    StopCommand = ReactiveCommand.CreateFromTask(StopTask);

    StartSpecificCommand = ReactiveCommand.CreateFromTask<TaskRowViewModel>(StartSpecificTask);
    StopSpecificCommand = ReactiveCommand.CreateFromTask<TaskRowViewModel>(StopSpecificTask);

    StartAllCommand = ReactiveCommand.CreateFromTask(StartAllTasks);
    StopAllCommand = ReactiveCommand.CreateFromTask(StopAllTasks);
    DuplicateCommand = ReactiveCommand.CreateFromTask(DuplicateTask);
    RemoveGroupCommand = ReactiveCommand.CreateFromTask(RemoveTaskGroup);

    SaveCommand = ReactiveCommand.CreateFromTask<CheckoutTaskModel>(Save);
    taskEditor.SaveChangesCommand
      .Where(it => it is not null)
      .InvokeCommand(SaveCommand!)
      .DisposeWith(Disposable);
    //
    // taskEditor.CancelCommand
    //   .Subscribe(_ => _hostScreen.Router.NavigateBack.Execute().Subscribe())
    //   .DisposeWith(Disposable);

    SelectedRows.GetWeakCollectionChangedObservable()
      .Select(_ =>
      {
        if (SelectedRows.Count > 0 && SelectedRows.Count != _tasks?.Count)
        {
          return (bool?)null;
        }

        return SelectedRows.Count == _tasks?.Count;
      })
      .ObserveOn(RxApp.MainThreadScheduler)
      .ToPropertyEx(this, _ => _.IsSelectedAllTasks);
    // _taskEditor.CancelCommand.IsExecuting
    //   .Merge(_taskEditor.SaveChangesCommand.IsExecuting)
    //   .Where(executing => executing)
    //   .Take(1)
    //   .Subscribe(_ => _router.NavigateBack.Execute().Subscribe());
  }

  private async Task RemoveTaskGroup(CancellationToken ct)
  {
    await _tasksService.RemoveGroup(TaskGroup, ct);
    _toasts.Show(ToastContent.Success("Group was removed successfully"));
  }

  private TaskGroupViewModel InitializeWith(CheckoutTaskGroupModel group, IReadonlyDependencyResolver resolver)
  {
    TaskGroup = group;
    var filter = this.WhenAnyValue(_ => _.SearchTerm)
      .Throttle(TimeSpan.FromMilliseconds(200))
      // .Where(s => string.IsNullOrEmpty(s) || s.Length >= MinSearchTermLen)
      .Select(_ => TasksFilter);

    var tasks = group.Tasks
      .Connect()
      // .Throttle(TimeSpan.FromMilliseconds(60))
      .Transform((t, _) => TaskRowViewModel.Create(t, resolver))
      .Replay(1)
      .RefCount();

    tasks
      .Filter(filter)
      .SortBy(_ => _.Task.CreatedAt, SortDirection.Descending)
      .ObserveOn(RxApp.MainThreadScheduler)
      .Bind(out _tasks)
      .DisposeMany()
      .Subscribe(_ =>
      {
        SelectedRows.Clear();
        if (_tasks.Count > 0)
        {
          SelectedRows.Add(_tasks[0]);
        }
      })
      .DisposeWith(Disposable);

    var statusChanges = tasks
      .AutoRefresh(_ => _.Status)
      .Transform(_ => _.Status ?? TaskStatusData.Idle, true)
      .ToCollection()
      .Replay(1)
      .RefCount();

    statusChanges
      .Select(_ => _.Count(it => it.Category is TaskCategory.CheckedOut))
      .DistinctUntilChanged()
      .ObserveOn(RxApp.MainThreadScheduler)
      .ToPropertyEx(this, _ => _.CheckedOutCount)
      .DisposeWith(Disposable);

    statusChanges
      .Select(_ => _.Count(it => it.Category is TaskCategory.Declined))
      .DistinctUntilChanged()
      .ObserveOn(RxApp.MainThreadScheduler)
      .ToPropertyEx(this, _ => _.DeclinedCount)
      .DisposeWith(Disposable);

    statusChanges
      .Select(_ => _.Count(it => it.Category is TaskCategory.Carted))
      .DistinctUntilChanged()
      .ObserveOn(RxApp.MainThreadScheduler)
      .ToPropertyEx(this, _ => _.CartedCount)
      .DisposeWith(Disposable);
    //
    // statusChanges
    //   // .Filter(s => s!.Category == TaskStatusCategory.Success && s.Stage == CheckoutStage.Checkout)
    //   .ToCollection()
    //   .Select(_ => _.Count)
    //   // .DistinctUntilChanged()
    //   .Do(count => Console.WriteLine(count))
    //   .ObserveOn(RxApp.MainThreadScheduler);

    return this;
  }

  public static TaskGroupViewModel Create(CheckoutTaskGroupModel group, IReadonlyDependencyResolver resolver)
  {
    return resolver.GetService<TaskGroupViewModel>()!
      .InitializeWith(group, resolver);
  }

  private Func<TaskRowViewModel, bool> TasksFilter => t =>
  {
    if (string.IsNullOrEmpty(SearchTerm))
    {
      return true;
    }

    return t.Module.ToString().Contains(SearchTerm, StringComparison.InvariantCultureIgnoreCase)
           || t.ProfileName.Contains(SearchTerm, StringComparison.InvariantCultureIgnoreCase)
           || t.Task.ProductSku.Contains(SearchTerm, StringComparison.InvariantCultureIgnoreCase);
  };

  private async Task StopSpecificTask(TaskRowViewModel task, CancellationToken ct)
  {
    await _taskViewService.StopTask(new[] { task }, ct);
    _toasts.Show(ToastContent.Success("Task stopped"));
  }

  private async Task StartSpecificTask(TaskRowViewModel task, CancellationToken ct)
  {
    var result = await _taskViewService.StartTask(new[] { task }, ct);
    ToastContent message = result.IsFailure
      ? ToastContent.Error(result.Error)
      : ToastContent.Success("Task started");
    _toasts.Show(message);
  }

  private async Task DuplicateTask(CancellationToken ct)
  {
    if (SelectedRows.Count == 0)
    {
      _toasts.Show(ToastContent.Warning("Nothing selected"));
      return;
    }

    var encounteredErrors = new List<string>();
    using (_busyIndicatorFactory.SwitchToBusyState())
    {
      using var __ = SelectedRows.SuspendNotifications();
      var result = await _taskViewService.Duplicate(TaskGroup.Id, SelectedRows.Select(_ => _.Task).ToArray(), ct);
      if (result.IsFailure)
      {
        encounteredErrors.Add(result.Error);
      }
    }

    ToastContent message = encounteredErrors.Any()
      ? ToastContent.Error(string.Join("\n", encounteredErrors))
      : ToastContent.Success("Task duplicated successfully");

    _toasts.Show(message);
  }

  private void OpenCreateTaskEditor()
  {
    _taskEditor.EditTask(new CheckoutTaskModel());

    // _hostScreen.Router.Navigate.Execute(_taskEditor).Subscribe();
  }

  private async Task StopTask(CancellationToken ct)
  {
    if (SelectedRows.Count == 0)
    {
      _toasts.Show(ToastContent.Warning("Nothing selected"));
      return;
    }

    using var _ = _busyIndicatorFactory.SwitchToBusyState();
    using var __ = SelectedRows.SuspendNotifications();
    await _taskViewService.StopTask(SelectedRows.ToArray(), ct);

    _toasts.Show(ToastContent.Success("Task stopped"));
  }

  private async Task StopAllTasks(CancellationToken ct)
  {
    using (_busyIndicatorFactory.SwitchToBusyState())
    {
      await _taskViewService.StopTask(_tasks.ToArray(), ct);
    }

    _toasts.Show(ToastContent.Success("All tasks stopped"));
  }

  private async Task StartTask(CancellationToken ct)
  {
    if (SelectedRows.Count == 0)
    {
      _toasts.Show(ToastContent.Warning("Nothing selected"));
      return;
    }

    var encounteredErrors = new List<string>();
    using (_busyIndicatorFactory.SwitchToBusyState())
    {
      using var _ = SelectedRows.SuspendNotifications();
      var result = await _taskViewService.StartTask(SelectedRows.ToArray(), ct);
      if (result.IsFailure)
      {
        encounteredErrors.Add(result.Error);
      }
    }

    ToastContent message = encounteredErrors.Any()
      ? ToastContent.Error(string.Join("\n", encounteredErrors))
      : ToastContent.Success("Task started");

    _toasts.Show(message);
  }

  private async Task StartAllTasks(CancellationToken ct)
  {
    var someFailedToStart = false;
    using (_busyIndicatorFactory.SwitchToBusyState())
    {
      var r = await _taskViewService.StartTask(_tasks.Where(_ => !_.IsRunning).ToArray(), ct);
      someFailedToStart = someFailedToStart || r.IsFailure;

      // var tasks = _tasks.Where(_ => !_.IsRunning).Select(row => _taskViewService.StartTask(row, ct).AsTask());
      // await Task.WhenAll(tasks);
    }

    var content = someFailedToStart
      ? ToastContent.Warning("Some tasks are invalid")
      : ToastContent.Success("All tasks started");

    _toasts.Show(content);
  }

  private void OpenEditTaskEditor()
  {
    if (SelectedRows.Count == 0)
    {
      return;
    }

    _taskEditor.EditTask(SelectedRows[0].Task);
    // _router.Navigate.Execute(_taskEditor).Subscribe();
  }

  private async Task RemoveTask()
  {
    if (SelectedRows.Count == 0)
    {
      return;
    }

    using var _ = SelectedRows.SuspendNotifications();
    await RemoveTasks(SelectedRows);
  }

  private async Task RemoveAllTasks()
  {
    await RemoveTasks(Tasks);
  }

  private async Task RemoveTasks(IEnumerable<TaskRowViewModel> toRemove)
  {
    /* NOTICE: need to create copy because enumerator will throw */
    using var __ = SelectedRows.SuspendNotifications();
    var hasErrors = false;
    var result = await _tasksService.BulkRemove(TaskGroup.Id, toRemove.Select(_ => _.Task).ToArray());
    hasErrors = hasErrors || result.IsFailure;

    ToastContent notification = hasErrors
      ? ToastContent.Error("Some tasks failed to remove")
      : ToastContent.Success("All tasks were removed successfully");

    _toasts.Show(notification);
  }

  private async Task Save(CheckoutTaskModel task)
  {
    var hasErrors = false;
    using var __ = SelectedRows.SuspendNotifications();
    using var ___ = _taskEditor.SwitchToBusyState();
    if (task.Id == default)
    {
      var saveResult = await _taskViewService.Create(TaskGroup.Id, task, _taskEditor.TaskCountToCreate);
      if (saveResult.IsFailure)
      {
        _toasts.Show(ToastContent.Error(saveResult.Error));
        hasErrors = true;
      }
    }
    else
    {
      var proto = _mapper.Map<CheckoutTaskModel>(task);
      var saveResult =
        await _taskViewService.Save(TaskGroup.Id, SelectedRows.Select(_ => _mapper.Map(proto, _.Task)).ToArray());
      if (saveResult.IsFailure)
      {
        _toasts.Show(ToastContent.Error(saveResult.Error));
        hasErrors = true;
      }
    }

    _taskEditor.EditTask(null);
    // _hostScreen.Router.NavigateBack.Execute().Subscribe();
    _toasts.Show(hasErrors
      ? ToastContent.Error("Error on tasks creation")
      : ToastContent.Success("Task saved successfully"));
  }

  public TaskEditorViewModel Editor => _taskEditor;

  public CheckoutTaskGroupModel TaskGroup { get; private set; } = null!;
  [Reactive] public ObservableCollectionExtended<TaskRowViewModel> SelectedRows { get; private set; } = new();

  public ReadOnlyObservableCollection<TaskRowViewModel> Tasks => _tasks;

  public int CheckedOutCount { [ObservableAsProperty] get; }
  public int DeclinedCount { [ObservableAsProperty] get; }
  public int CartedCount { [ObservableAsProperty] get; }

  [Reactive] public string? SearchTerm { get; set; }
  public ReactiveCommand<Unit, Unit> RemoveGroupCommand { get; }
  public ReactiveCommand<Unit, Unit> RemoveCommand { get; }
  public ReactiveCommand<Unit, Unit> RemoveAllCommand { get; }
  public ReactiveCommand<Unit, Unit> EditCommand { get; }
  public ReactiveCommand<Unit, Unit> DuplicateCommand { get; }
  public ReactiveCommand<CheckoutTaskModel, Unit> SaveCommand { get; }
  public ReactiveCommand<Unit, Unit> CreateTaskCommand { get; }
  public ReactiveCommand<Unit, Unit> StartCommand { get; }
  public ReactiveCommand<TaskRowViewModel, Unit> StartSpecificCommand { get; }
  public ReactiveCommand<TaskRowViewModel, Unit> StopSpecificCommand { get; }
  public ReactiveCommand<Unit, Unit> StopCommand { get; }
  public ReactiveCommand<Unit, Unit> StartAllCommand { get; }
  public ReactiveCommand<Unit, Unit> StopAllCommand { get; }
  public ReactiveCommand<Unit, Unit> ToggleSelectAllTasksCommand { get; }
  public ReactiveCommand<TaskRowViewModel, Unit> ToggleSelectedTaskCommand { get; }

  public bool? IsSelectedAllTasks { [ObservableAsProperty] get; }
}