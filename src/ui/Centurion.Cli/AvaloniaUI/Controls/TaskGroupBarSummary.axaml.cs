using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Centurion.Cli.Core;
using Centurion.Cli.Core.Domain.Tasks;
using Centurion.Cli.Core.Services.Tasks;
using DynamicData;
using ReactiveUI;
using Splat;

namespace Centurion.Cli.AvaloniaUI.Controls;

public class TaskGroupBarSummary : ContentControl
{
  private readonly CompositeDisposable _disposable = new();
  private Grid? _chartContainer;
  private StackPanel? _runningLegend;

  public static readonly StyledProperty<CheckoutTaskGroupModel?> TaskGroupProperty =
    AvaloniaProperty.Register<TaskGroupBarSummary, CheckoutTaskGroupModel?>(nameof(TaskGroup));

  public static readonly StyledProperty<int> TotalActiveCountProperty =
    AvaloniaProperty.Register<TaskGroupBarSummary, int>(nameof(TotalActiveCount));

  public static readonly DirectProperty<TaskGroupBarSummary, string?> TaskGroupNameProperty =
    AvaloniaProperty.RegisterDirect<TaskGroupBarSummary, string?>(nameof(TaskGroupName), _ => _.TaskGroupName);


  static TaskGroupBarSummary()
  {
    TaskGroupProperty.Changed
      .Subscribe(e =>
      {
        if (e.Sender is not TaskGroupBarSummary { IsInitialized: true } bs)
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
    if (_chartContainer is null || _runningLegend is null)
    {
      // throw new InvalidOperationException("Control is not initialized yet");
      return;
    }

    var active = 0;
    var stateInfo = new SortedDictionary<TaskProgress, int>
    {
      [TaskProgress.CheckOut] = 0,
      [TaskProgress.Decline] = 0,
      [TaskProgress.Cart] = 0,
      [TaskProgress.Error] = 0,
      [TaskProgress.Running] = 0,
      [TaskProgress.Idle] = 0,
    };

    foreach (var task in tasks)
    {
      var progress = task.Status.ToProgress();
      if (progress is not TaskProgress.Idle)
      {
        active++;
      }

      stateInfo[progress]++;
    }

    stateInfo[TaskProgress.Idle] = tasks.Count - active;
    _chartContainer.Children.Clear();
    _chartContainer.ColumnDefinitions.Clear();
    _runningLegend.Children.Clear();

    var barsToInsert = new List<Border>(stateInfo.Count);
    var legendToInsert = new List<Control>(stateInfo.Count);
    foreach (var (status, count) in stateInfo)
    {
      if (count == 0)
      {
        continue;
      }

      if (!this.TryFindResource(status + "Brush", out var foundBrushResource)
          || foundBrushResource is not SolidColorBrush colorBrush)
      {
        throw new InvalidOperationException("Can't find brush for status " + status);
      }

      _chartContainer.ColumnDefinitions.Add(new ColumnDefinition(count, GridUnitType.Star));
      var segment = new Border
      {
        Classes = { "TaskStatusBarItem" },
        Background = colorBrush
      };

      barsToInsert.Add(segment);
      Grid.SetColumn(segment, _chartContainer.ColumnDefinitions.Count - 1);

      if (status is TaskProgress.Idle)
      {
        continue;
      }

      if (legendToInsert.Count > 0 && legendToInsert.Count % 2 != 0)
      {
        legendToInsert.Add(new Ellipse
        {
          Classes = { "LegendItemDelim" }
        });
      }

      legendToInsert.Add(new TextBlock
      {
        Text = $"{count} {status}",
        Classes = { "LegendItem", },
        Foreground = colorBrush
      });
    }

    barsToInsert.LastOrDefault()?.Classes.Add("LastChild");
    TotalActiveCount = active;
    _chartContainer.Children.AddRange(barsToInsert);
    _runningLegend.Children.AddRange(legendToInsert);
  }


  protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
  {
    base.OnApplyTemplate(e);

    _chartContainer = e.NameScope.Get<Grid>("PART_ChartContainer");
    _runningLegend = e.NameScope.Get<StackPanel>("PART_RunningLegendContainer");
    if (TaskGroup is not null)
    {
      UpdateTaskGroupInfo();
    }
  }

  public int TotalActiveCount
  {
    get => GetValue(TotalActiveCountProperty);
    set => SetValue(TotalActiveCountProperty, value);
  }


  public string? TaskGroupName => TaskGroup?.Name;

  public CheckoutTaskGroupModel? TaskGroup
  {
    get => GetValue(TaskGroupProperty);
    set => SetValue(TaskGroupProperty, value);
  }
}