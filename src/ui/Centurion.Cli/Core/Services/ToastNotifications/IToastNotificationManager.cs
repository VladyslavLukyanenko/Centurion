using DynamicData;
using NodaTime;

namespace Centurion.Cli.Core.Services.ToastNotifications;

public class ToastEntry
{
  public ToastEntry(ToastContent content)
  {
    Content = content;
    Timestamp = SystemClock.Instance.GetCurrentInstant();
    Id = Guid.NewGuid();
  }

  public Guid Id { get; private set; }
  public ToastContent Content { get; private set; }
  public Instant Timestamp { get; private set; }
}

public interface IToastNotificationManager
{
  void Suspend();
  void Resume();
  void Clear();

  void ClearAllImportant();
  void Show(ToastContent content);

  IObservableCache<ToastEntry, Guid> ImportantNotifications { get; }
}