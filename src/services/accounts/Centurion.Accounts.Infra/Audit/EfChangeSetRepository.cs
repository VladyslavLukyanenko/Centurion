using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.Audit;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Infra.Repositories;

namespace Centurion.Accounts.Infra.Audit;

public class EfChangeSetRepository
  : EfCrudRepository<ChangeSet, Guid>, IChangeSetRepository
{
  public EfChangeSetRepository(DbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
  {
  }

  private IQueryable<ChangeSetEntry> ChangeSetEntries => DataSource.AsNoTracking().SelectMany(_ => _.Entries);

  public Task<ChangeSetEntry?> GetPreviousAsync(ChangeSetEntry current, CancellationToken ct = default)
  {
    return ChangeSetEntries
      .Where(_ => _.EntityId == current.EntityId && _.EntityType == current.EntityType &&
                  _.CreatedAt < current.CreatedAt)
      .OrderByDescending(_ => _.CreatedAt)
      .FirstOrDefaultAsync(ct)!;
  }

  public Task<ChangeSetEntry?> GetEntryByIdAsync(Guid changeSetEntryId, CancellationToken ct = default)
  {
    return ChangeSetEntries.FirstOrDefaultAsync(_ => _.Id == changeSetEntryId, ct)!;
  }
}