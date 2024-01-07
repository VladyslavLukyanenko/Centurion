using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Centurion.Cli.Core.Domain.Tasks;
using Centurion.Cli.Core.Services.Tasks;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace Centurion.Cli.Core.ViewModels.Tasks;

public class TasksViewModel : ViewModelBase, IRoutableViewModel
{
  private readonly ReadOnlyObservableCollection<TaskGroupViewModel> _taskGroups;

#if DEBUG
  public TasksViewModel()
  {
  }
#endif

  public TasksViewModel(IScreen hostScreen, ITasksService tasksService, IReadonlyDependencyResolver resolver)
  {
    HostScreen = hostScreen;

    tasksService.TaskGroups
      .Connect()
      .ObserveOn(RxApp.MainThreadScheduler)
      .Transform((t, _) => TaskGroupViewModel.Create(t, resolver))
      .Sort(SortExpressionComparer<TaskGroupViewModel>.Ascending(_ => _.TaskGroup.CreatedAt)
        .ThenByAscending(_ => _.TaskGroup.Id))
      .DisposeMany()
      .Bind(out _taskGroups)
      .Subscribe()
      .DisposeWith(Disposable);

    CreateGroupCommand = ReactiveCommand.Create(() =>
    {
      if (EditingGroup is not null && EditingGroup.Id == Guid.Empty)
      {
        return;
      }

      EditingGroup = new CheckoutTaskGroupModel();
    });

    EditSelectedCommand = ReactiveCommand.Create(() =>
    {
      if (SelectedGroup is null)
      {
        return;
      }

      EditingGroup = SelectedGroup.TaskGroup;
    });

    SaveCommand = ReactiveCommand.CreateFromTask(async ct =>
    {
      if (EditingGroup is null)
      {
        return;
      }

      await tasksService.SaveGroup(EditingGroup, ct);
      EditingGroup = new CheckoutTaskGroupModel();
    });


    RemoveCommand = ReactiveCommand.CreateFromTask(async ct =>
    {
      if (SelectedGroup is null)
      {
        return;
      }

      await tasksService.RemoveGroup(SelectedGroup.TaskGroup, ct);
    });

    EditTasksCommand = ReactiveCommand.CreateFromTask(async () =>
    {
      if (SelectedGroup is null)
      {
        return;
      }

      SelectedGroup.SelectedRows.Clear();
      await SelectedGroup.ToggleSelectAllTasksCommand.Execute();
      await SelectedGroup.EditCommand.Execute();
    });

    StartTasksCommand = ReactiveCommand.CreateFromTask(async () =>
    {
      if (SelectedGroup is null)
      {
        return;
      }

      await SelectedGroup.StartAllCommand.Execute();
    });

    StopTasksCommand = ReactiveCommand.CreateFromTask(async () =>
    {
      if (SelectedGroup is null)
      {
        return;
      }

      await SelectedGroup.StopAllCommand.Execute();
    });
  }

  [Reactive] public TaskGroupViewModel? SelectedGroup { get; set; }
  [Reactive] public CheckoutTaskGroupModel? EditingGroup { get; set; }

  public ReadOnlyObservableCollection<TaskGroupViewModel> TaskGroups => _taskGroups;

  public ReactiveCommand<Unit, Unit> EditSelectedCommand { get; }
  public ReactiveCommand<Unit, Unit> CreateGroupCommand { get; }
  public ReactiveCommand<Unit, Unit> SaveCommand { get; }
  public ReactiveCommand<Unit, Unit> RemoveCommand { get; }

  public ReactiveCommand<Unit, Unit> StartTasksCommand { get; }
  public ReactiveCommand<Unit, Unit> StopTasksCommand { get; }
  public ReactiveCommand<Unit, Unit> EditTasksCommand { get; }

  public string UrlPathSegment => nameof(TasksViewModel);
  public IScreen HostScreen { get; }

}