using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Infra.Repositories;

public abstract class EfCrudRepository<T> : EfCrudRepository<T, long>
  where T : class, IEntity<long>, IEventSource
{
  protected EfCrudRepository(DbContext context, IUnitOfWork unitOfWork)
    : base(context, unitOfWork)
  {
  }
}