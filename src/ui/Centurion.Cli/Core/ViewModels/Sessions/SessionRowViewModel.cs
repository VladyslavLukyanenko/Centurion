using System.Reactive.Disposables;
using System.Reactive.Linq;
using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Domain.Accounts;
using Centurion.Contracts;
using DynamicData;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels.Sessions;

public class SessionRowViewModel : ViewModelBase
{
  public SessionRowViewModel(SessionModel session, IObservableCache<Account, Guid> accounts)
  {
    SourceSession = session;
    accounts.WatchValue(session.AccountId)
      .Select(_ => _.Email)
      .ToPropertyEx(this, _ => _.AccountName)
      .DisposeWith(Disposable);
  }

  public string Name => SourceSession.Name;
  public string AccountName { [ObservableAsProperty] get; } = null!;
  public string Status => SourceSession.Status.ToString();
  public Module Module => SourceSession.Module;
  public SessionModel SourceSession { get; }
}