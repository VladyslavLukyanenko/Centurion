using System.Data;
using Centurion.SeedWork.Infra.EfCoreNpgsql.Events;
using Centurion.SeedWork.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Centurion.SeedWork.Infra.EfCoreNpgsql.Services;

public class DbContextBasedUnitOfWork : IUnitOfWork
{
  private readonly DbContext _context;
  private readonly IMediator _mediator;

  public DbContextBasedUnitOfWork(DbContext context, IMediator mediator)
  {
    _context = context;
    _mediator = mediator;
  }

  public async ValueTask<int> SaveChangesAsync(CancellationToken token = default)
  {
    if (!_context.ChangeTracker.HasChanges())
    {
      return 0;
    }

    await _mediator.Publish(new DbContextBeforeEntitiesSaved(_context), token);
    return await _context.SaveChangesAsync(token);
  }

  public async ValueTask<bool> SaveEntitiesAsync(CancellationToken token = default)
  {
    await _mediator.DispatchDomainEvents(_context);
    await SaveChangesAsync(token);
    return true;
  }

  public async ValueTask<ITransactionScope> BeginTransactionAsync(bool autoCommit = true,
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