using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Centurion.Cli.Core.Domain.Tasks;
using Centurion.Cli.Core.Services.Tasks;
using Centurion.Contracts;
using DynamicData;
using ReactiveUI;
using Splat;

namespace Centurion.Cli.AvaloniaUI.Controls;

public class TaskGroupStatusBreakdown : ContentControl
{
  private readonly CompositeDisposable _disposable = new();

  private readonly Dictionary<TaskStage, TaskStageBreakdown> _breakdowns = Enum.GetValues<TaskStage>()
    .Where(it => it is not TaskStage.Unspecified)
    .ToDictionary(s => s, s => new TaskStageBreakdown(s.ToString("G")));

  public static readonly StyledProperty<CheckoutTaskGroupModel?> TaskGroupProperty =
    AvaloniaProperty.Register<TaskGroupStatusBreakdown, CheckoutTaskGroupModel?>(nameof(TaskGroup));

  public static readonly DirectProperty<TaskGroupStatusBreakdown, IReadOnlyCollection<TaskStageBreakdown>>
    BreakdownsProperty =
      AvaloniaProperty.RegisterDirect<TaskGroupStatusBreakdown, IReadOnlyCollection<TaskStageBreakdown>>(
        nameof(Breakdowns), _ => _.Breakdowns);

  static TaskGroupStatusBreakdown()
  {
    TaskGroupProperty.Changed
      .Subscribe(e =>
      {
        if (e.Sender is not TaskGroupStatusBreakdown { IsInitialized: true } bs)
        {
          return;
        }

        bs.UpdateTaskGroupInfo();
      });
  }

  protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
  {
    base.OnAttachedToVisualTree(e);
    if (TaskGroup is not null)
    {
      UpdateTaskGroupInfo();
    }
  }

  protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
  {
    base.OnDetachedFromVisualTree(e);
    _disposable.Clear();
  }

  private void UpdateTaskGroupInfo()
  {
    _disposable.Clear();
    if (TaskGroup is null)
    {
      return;
    }

    var statuses = Locator.Current.GetService<ITaskStatusRegistry>()!.Statuses;
    TaskGroup.Tasks.Connect()
      .Transform(t => new StatusAwareTask(statuses.WatchValue(t.Id).Select(_ => _.Status), t), true)
      .AutoRefresh(_ => _.Status)
      // .Sample(TimeSpan.FromMilliseconds(250), RxApp.TaskpoolScheduler)
      .DisposeMany()
      .ToCollection()
      .ObserveOn(RxApp.MainThreadScheduler)
      // .Select(c => c.GroupBy(_ => _.Status.Category).ToDictionary(_ => _.Key))
      .Subscribe(BuildCharts)
      .DisposeWith(_disposable);
  }


  private void BuildCharts(IReadOnlyCollection<StatusAwareTask> tasks)
  {
    var lp = tasks.ToLookup(_ => _.Status.Stage);
    foreach (var (stage, bd) in _breakdowns)
    {
      bd.UpdateLines(lp[stage]);
    }
  }

  protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
  {
    base.OnApplyTemplate(e);
    if (TaskGroup is not null)
    {
      UpdateTaskGroupInfo();
    }
  }

  public IReadOnlyCollection<TaskStageBreakdown> Breakdowns => _breakdowns.Values;

  public CheckoutTaskGroupModel? TaskGroup
  {
    get => GetValue(TaskGroupProperty);
    set => SetValue(TaskGroupProperty, value);
  }
}