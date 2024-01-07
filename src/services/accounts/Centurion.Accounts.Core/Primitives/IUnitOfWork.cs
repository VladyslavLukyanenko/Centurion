using System.Data;

namespace Centurion.Accounts.Core.Primitives;

public interface IUnitOfWork
  : IDisposable
{
  Task<int> SaveChangesAsync(CancellationToken token = default);

  Task<bool> SaveEntitiesAsync(CancellationToken token = default);

  Task<ITransactionScope> BeginTransactionAsync(bool autoCommit = true,
    IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken ct = default);
}