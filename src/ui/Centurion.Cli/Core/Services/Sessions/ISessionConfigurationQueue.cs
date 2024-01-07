using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Domain.Accounts;

namespace Centurion.Cli.Core.Services.Sessions;

public interface ISessionConfigurationQueue
{
  ValueTask Enqueue(SessionModel session, Account account);
  bool IsQueued(SessionModel session);
  IObservable<SessionModel> SessionProcessed { get; }
}