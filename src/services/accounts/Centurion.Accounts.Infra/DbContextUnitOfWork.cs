using System.Data;
using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.Events;
using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Infra;

public class DbContextUnitOfWork : IUnitOfWork
{
  private readonly DbContext _context;
  private readonly IMessageDispatcher _messageDispatcher;

  public DbContextUnitOfWork(DbContext context, IMessageDispatcher messageDispatcher)
  {
    _context = context;
    _messageDispatcher = messageDispatcher;
  }

  public Task<int> SaveChangesAsync(CancellationToken token = default)
  {
    return _context.SaveChangesAsync(token);
  }

  public async Task<bool> SaveEntitiesAsync(CancellationToken token = default)
  {
    await _messageDispatcher.DispatchDomainEvents(_context);
    await SaveChangesAsync(token);
    return true;
  }

  public async Task<ITransactionScope> BeginTransactionAsync(bool autoCommit = true,
    IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken ct = default)
  {
    var tx = await _context.Database.BeginTransactionAsync(isolationLevel, ct);
    return new EfCoreTransactionScope(tx, autoCommit);
  }

  public void Dispose()
  {
    _context.Dispose();
  }
}