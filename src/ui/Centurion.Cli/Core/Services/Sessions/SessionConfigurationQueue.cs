using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Domain.Accounts;
using Centurion.Cli.Core.Services.ToastNotifications;
using Centurion.Contracts;

namespace Centurion.Cli.Core.Services.Sessions;

public class SessionConfigurationQueue : ISessionConfigurationQueue, IAppBackgroundWorker, IDisposable
{
  private record QueuedSession(SessionModel Session, Account Account);

  private readonly BlockingCollection<QueuedSession> _pendingSessions = new();
  private readonly CancellationTokenSource _cts = new();
  private readonly ISessionAutoconfigurator _autoconfigurator;
  private readonly IToastNotificationManager _toasts;
  private readonly ISessionRepository _sessionRepository;
  private readonly Subject<SessionModel> _processed = new();
  private SessionModel? _currentlyProcessing;

  public SessionConfigurationQueue(ISessionAutoconfigurator autoconfigurator, IToastNotificationManager toasts,
    ISessionRepository sessionRepository)
  {
    _autoconfigurator = autoconfigurator;
    _toasts = toasts;
    _sessionRepository = sessionRepository;
    SessionProcessed = _processed.AsObservable();
  }

  public async ValueTask Enqueue(SessionModel session, Account account)
  {
    session.Status = SessionStatus.NotReady;
    _pendingSessions.Add(new QueuedSession(session, account));
    await _sessionRepository.SaveAsync(session);
  }

  public bool IsQueued(SessionModel session)
  {
    return _currentlyProcessing?.Id == session.Id || _pendingSessions.Any(m => m.Session.Id == session.Id);
  }

  public IObservable<SessionModel> SessionProcessed { get; }

  public void Spawn()
  {
    _ = Task.Run(async () =>
    {
      foreach (var (session, acc) in _pendingSessions.GetConsumingEnumerable(_cts.Token))
      {
        _currentlyProcessing = session;
        try
        {
          var cookies = await _autoconfigurator.Configure(acc, _cts.Token);
          session.Cookies = new HashSet<string>(cookies);
          session.Status = SessionStatus.Ready;
          await _sessionRepository.SaveAsync(session, _cts.Token);
          _toasts.Show(ToastContent.Success($"Session '{session.Name}' configured"));
        }
        catch
        {
          _toasts.Show(ToastContent.Error($"Failed to configure '{session.Name}'"));
        }
        finally
        {
          _currentlyProcessing = null;
          _processed.OnNext(session);
        }
      }
    }, _cts.Token);
  }

  public void Dispose()
  {
    _cts.Cancel();
  }
}