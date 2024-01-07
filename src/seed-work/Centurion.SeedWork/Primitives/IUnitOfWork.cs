using System.Data;

namespace Centurion.SeedWork.Primitives;

public interface IUnitOfWork
  : IDisposable
{
  ValueTask<int> SaveChangesAsync(CancellationToken token = default);

  ValueTask<bool> SaveEntitiesAsync(CancellationToken token = default);

  ValueTask<ITransactionScope> BeginTransactionAsync(bool autoCommit = true,
    IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken ct = default);
}