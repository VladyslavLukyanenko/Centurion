using NodaTime;
using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Infra;

// ReSharper disable once InconsistentNaming
public static class IQueryableExtensions
{
  public static IQueryable<T> WhereNotRemoved<T>(this IQueryable<T> src)
    where T: class, ISoftRemovable
  {
    return src.Where(_ => _.RemovedAt == Instant.MaxValue);
  }
}