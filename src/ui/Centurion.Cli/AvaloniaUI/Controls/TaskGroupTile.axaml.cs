using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Centurion.Cli.Core.Domain.Tasks;
using Centurion.Cli.Core.Services.Tasks;
using Centurion.Contracts;
using DynamicData;
using ReactiveUI;
using Splat;

namespace Centurion.Cli.AvaloniaUI.Controls;

public class TaskGroupTile : ContentControl
{
  private readonly CompositeDisposable _disposable = new();

  /// <summary>
  /// Defines the <see cref="IsSelected"/> property.
  /// </summary>
  public static readonly StyledProperty<bool> IsSelectedProperty =
    AvaloniaProperty.Register<TaskGroupTile, bool>(nameof(IsSelected));

  /// <summary>
  /// Defines the <see cref="Picture"/> property.
  /// </summary>
  public static readonly StyledProperty<string?> PictureProperty =
    AvaloniaProperty.Register<TaskGroupTile, string?>(nameof(Picture));


  public static readonly StyledProperty<int> CheckedOutCountProperty =
    AvaloniaProperty.Register<TaskGroupTile, int>(nameof(CheckedOutCount));

  public static readonly StyledProperty<int> DeclinedCountProperty =
    AvaloniaProperty.Register<TaskGroupTile, int>(nameof(DeclinedCount));

  public static readonly StyledProperty<int> CartedCountProperty =
    AvaloniaProperty.Register<TaskGroupTile, int>(nameof(CartedCount));

  public static readonly StyledProperty<int> TasksCountProperty =
    AvaloniaProperty.Register<TaskGroupTile, int>(nameof(TasksCount));


  public static readonly StyledProperty<CheckoutTaskGroupModel?> TaskGroupProperty =
    AvaloniaProperty.Register<TaskGroupTile, CheckoutTaskGroupModel?>(nameof(TaskGroup));

  static TaskGroupTile()
  {
    TaskGroupProperty.Changed.Subscribe(e =>
    {
      if (e.Sender is not TaskGroupTile tgl)
      {
        return;
      }

      tgl.OnTaskGroupChanged();
    });
  }

  private void OnTaskGroupChanged()
  {
    _disposable.Clear();
    if (TaskGroup is null)
    {
      return;
    }

    var statuses = Locator.Current.GetService<ITaskStatusRegistry>()!.Statuses;
    TaskGroup.Tasks.Connect()
      .AutoRefresh(_ => _.ProductPicture)
      .ObserveOn(RxApp.MainThreadScheduler)
      .Bind(out var tasks)
      .Subscribe(__ =>
      {
        Picture = tasks.Select(_ => _.ProductPicture).FirstOrDefault();
        TasksCount = tasks.Count;
      })
      .DisposeWith(_disposable);

   TaskGroup.Tasks.Connect()
      .Transform(t => new StatusAwareTask(statuses.WatchValue(t.Id).Select(_ => _.Status), t), true)
      .AutoRefresh(_ => _.Status)
      .Transform(_ => _.Status, true)
      .Bind(out var taskStatuses)
      .ObserveOn(RxApp.MainThreadScheduler)
      .Subscribe(_ =>
      {
        var checkedOut = 0;
        var declined = 0;
        var carted = 0;
        for (var ix = 0; ix < taskStatuses.Count; ix++)
        {
          var status = taskStatuses[ix];
          if (status.Category is TaskCategory.CheckedOut)
          {
            checkedOut++;
          }
          else if (status.Category is TaskCategory.Declined)
          {
            declined++;
          }
          else if (status.Category is TaskCategory.Carted)
          {
            carted++;
          }
        }

        CheckedOutCount = checkedOut;
        DeclinedCount = declined;
        CartedCount = carted;
      })
      .DisposeWith(_disposable);
  }

  protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
  {
    base.OnAttachedToVisualTree(e);
    OnTaskGroupChanged();
  }

  protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
  {
    _disposable.Clear();
    base.OnDetachedFromVisualTree(e);
  }

  public CheckoutTaskGroupModel? TaskGroup
  {
    get => GetValue(TaskGroupProperty);
    set => SetValue(TaskGroupProperty, value);
  }

  /// <summary>
  /// Gets or sets the selection state of the item.
  /// </summary>
  public bool IsSelected
  {
    get => GetValue(IsSelectedProperty);
    set => SetValue(IsSelectedProperty, value);
  }

  public int CheckedOutCount
  {
    get => GetValue(CheckedOutCountProperty);
    private set => SetValue(CheckedOutCountProperty, value);
  }

  public int DeclinedCount
  {
    get => GetValue(DeclinedCountProperty);
    private set => SetValue(DeclinedCountProperty, value);
  }

  public int CartedCount
  {
    get => GetValue(CartedCountProperty);
    private set => SetValue(CartedCountProperty, value);
  }

  public int TasksCount
  {
    get => GetValue(TasksCountProperty);
    private set => SetValue(TasksCountProperty, value);
  }

  public string? Picture
  {
    get => GetValue(PictureProperty);
    private set => SetValue(PictureProperty, value);
  }
}