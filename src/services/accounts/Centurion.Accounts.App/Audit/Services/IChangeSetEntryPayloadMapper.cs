using Centurion.Accounts.Core.Audit;

namespace Centurion.Accounts.App.Audit.Services;

public interface IChangeSetEntryPayloadMapper
{
  Task<IDictionary<string, string?>?> MapAsync(ChangeSetEntry entry, CancellationToken ct = default);
}