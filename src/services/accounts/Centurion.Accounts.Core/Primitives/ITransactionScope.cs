namespace Centurion.Accounts.Core.Primitives;

public interface ITransactionScope
  : IDisposable
{
  Task RollbackAsync(CancellationToken ct = default);
  Task CommitAsync(CancellationToken ct = default);
}