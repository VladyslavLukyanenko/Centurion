using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.Audit;

public interface IChangeSetRepository : IRepository<ChangeSet, Guid>
{
  Task<ChangeSetEntry?> GetPreviousAsync(ChangeSetEntry current, CancellationToken ct = default);
  Task<ChangeSetEntry?> GetEntryByIdAsync(Guid changeSetEntryId, CancellationToken ct = default);
}