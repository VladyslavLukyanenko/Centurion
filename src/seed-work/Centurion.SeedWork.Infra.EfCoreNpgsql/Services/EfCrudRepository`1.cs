using Centurion.SeedWork.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Centurion.SeedWork.Infra.EfCoreNpgsql.Services;

public abstract class EfCrudRepository<T>
  : EfCrudRepository<T, long>
  where T : class, IEntity<long>
{
  protected EfCrudRepository(DbContext context, IUnitOfWork unitOfWork)
    : base(context, unitOfWork)
  {
  }
}