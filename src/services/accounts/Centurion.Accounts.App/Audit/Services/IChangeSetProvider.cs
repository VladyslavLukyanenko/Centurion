using Centurion.Accounts.App.Audit.Data;
using Centurion.Accounts.Core.Collections;

namespace Centurion.Accounts.App.Audit.Services;

public interface IChangeSetProvider
{
  Task<IPagedList<ChangeSetData>> GetChangeSetPageAsync(ChangeSetPageRequest pageRequest,
    CancellationToken ct = default);

  Task<ChangesetEntryPayloadData?> GetPayloadAsync(Guid changeSetEntryId, CancellationToken ct = default);
}