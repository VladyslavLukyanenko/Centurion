using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Infra.Services;

public abstract class DataProvider
{
  protected readonly DbContext Context;

  protected DataProvider(DbContext context)
  {
    Context = context;
  }

  protected IQueryable<T> GetDataSource<T>() where T : class
  {
    return Context.Set<T>().AsNoTracking();
  }

  protected IQueryable<T> GetAliveDataSource<T>() where T : class, ISoftRemovable => GetDataSource<T>()
    .WhereNotRemoved();
}