using System.Reactive.Linq;
using Avalonia.Threading;
using Centurion.Cli.AvaloniaUI.Controls;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Services.ToastNotifications;
using DynamicData;
using ReactiveUI;

namespace Centurion.Cli.AvaloniaUI.Services;

public class AvaloniaUIToastNotificationsManager : IToastNotificationManager
{
  private const int VisibleNotificationsLimit = 7;

  private readonly IPrioritizedToastPublisher _prioritizedToastPublisher;
  private readonly LinkedList<NotificationToast> _spawnedToasts = new();
  private readonly INotificationsHostProvider _notificationsHostProvider;
  private readonly SourceCache<ToastEntry, Guid> _importantToasts = new(_ => _.Id);

  public AvaloniaUIToastNotificationsManager(IPrioritizedToastPublisher prioritizedToastPublisher,
    INotificationsHostProvider notificationsHostProvider)
  {
    _prioritizedToastPublisher = prioritizedToastPublisher;
    _notificationsHostProvider = notificationsHostProvider;
    ImportantNotifications = _importantToasts.AsObservableCache();
  }

  public void Suspend()
  {
  }

  public void Resume()
  {
  }

  public void Clear()
  {
    var host = _notificationsHostProvider.GetHost();
    host.Children.Clear();
  }

  public void ClearAllImportant()
  {
    _importantToasts.Clear();
  }

  public void Show(ToastContent content)
  {
    if (content.Priority == ToastPriority.Important)
    {
      _ = _prioritizedToastPublisher.PublishAsync(content);
      _importantToasts.AddOrUpdate(new ToastEntry(content));
      return;
    }

    Dispatcher.UIThread.InvokeAsync(() =>
    {
      var toast = new NotificationToast
      {
        Title = content.Title,
        MessageContent = content.Content,
        Type = content.Type,
        LifetimeDuration = content.AutoCloseTimeout,
        CustomIcon = content.CustomIcon,
        CustomRightContent = content.RightContent,
        CustomBottomContent = content.BottomContent
      };

      EnsureCanAddOneMoreToast();
      _spawnedToasts.AddLast(toast);

      var host = _notificationsHostProvider.GetHost();
      toast.CloseCommand = ReactiveCommand.Create(() =>
      {
        _spawnedToasts.Remove(toast);
        host.Children.Remove(toast);
      });

      toast.Command = toast.CloseCommand;

      Observable.Return(toast)
        .Take(1)
        .Delay(content.AutoCloseTimeout)
        .ObserveOn(RxApp.MainThreadScheduler)
        .Subscribe(t => { t.CloseCommand?.Execute(null); });

      host.Children.Add(toast);
    });
  }

  public IObservableCache<ToastEntry, Guid> ImportantNotifications { get; }

  private void EnsureCanAddOneMoreToast()
  {
    if (!_spawnedToasts.Any() || _spawnedToasts.Count + 1 < VisibleNotificationsLimit)
    {
      return;
    }

    _spawnedToasts.First?.Value.CloseCommand?.Execute(null);
  }
}