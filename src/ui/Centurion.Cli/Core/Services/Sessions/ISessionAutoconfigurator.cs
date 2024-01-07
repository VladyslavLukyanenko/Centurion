using Centurion.Cli.Core.Domain.Accounts;

namespace Centurion.Cli.Core.Services.Sessions;

public interface ISessionAutoconfigurator
{
  event EventHandler<OneclickAutomationStatus> StatusChanged;

  ValueTask<IList<string>> Configure(Account account, CancellationToken ct = default);
}