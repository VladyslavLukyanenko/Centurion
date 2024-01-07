using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls.Mixins;
using Centurion.Cli.AvaloniaUI.Controls;
using Centurion.Cli.Core.Services.ToastNotifications;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Centurion.Cli.Core.ViewModels;

public class NotificationsViewModel : ViewModelBase
{
  private readonly IToastNotificationManager _toasts;

  private readonly ReadOnlyObservableCollection<NotificationData> _notifications;

  public NotificationsViewModel(IToastNotificationManager toasts)
  {
    _toasts = toasts;

    toasts.ImportantNotifications.Connect()
      .SortBy(_ => _.Timestamp, SortDirection.Descending)
      .Transform(n => new NotificationData
      {
        Description = n.Content.Content,
        Timestamp = n.Timestamp,
        Title = n.Content.Title,
        Type = n.Content.Type.ToNotificationType()
      })
      .ObserveOn(RxApp.MainThreadScheduler)
      .Bind(out _notifications)
      .Subscribe()
      .DisposeWith(Disposable);

    ClearCommand = ReactiveCommand.Create(toasts.ClearAllImportant);
  }

  public ReadOnlyObservableCollection<NotificationData> Notifications => _notifications;

  public ReactiveCommand<Unit, Unit> ClearCommand { get; }
}