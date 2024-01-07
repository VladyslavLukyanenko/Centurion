using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.Collections;

namespace Centurion.Accounts.Infra;

public static class PageQueryBaseExtensions
{
  public static async Task<IPagedList<TSource>> PaginateAsync<TSource>(this IQueryable<TSource> initialQuery,
    IPageRequest request, CancellationToken token = default)
  {
    var count = await initialQuery.CountAsync(token);
    if (count == 0)
    {
      return PagedList<TSource>.Empty(request);
    }

    initialQuery = TryApplyOrderBy(initialQuery, request);

    var data = await initialQuery.Skip(request.Offset)
      .Take(request.Limit)
      .ToListAsync(token);

    return new PagedList<TSource>(data, count, request.Limit, request.PageIndex);
  }

  private static IQueryable<TSource> TryApplyOrderBy<TSource>(IQueryable<TSource> initialQuery,
    IPageRequest request)
  {
    if (!string.IsNullOrEmpty(request.OrderBy))
    {
      initialQuery = OrderingMethodFinder.OrderMethodExists(initialQuery.Expression)
        ? ((IOrderedQueryable<TSource>)initialQuery).ThenBy(request.OrderBy)
        : initialQuery.OrderBy(request.OrderBy);
    }

    return initialQuery;
  }

  // Adapted from internal System.Web.Util.OrderingMethodFinder http://referencesource.microsoft.com/#System.Web/Util/OrderingMethodFinder.cs
  class OrderingMethodFinder : ExpressionVisitor
  {
    bool _orderingMethodFound;

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
      var name = node.Method.Name;
      if ((node.Method.DeclaringType == typeof(Queryable)
           || node.Method.DeclaringType?.GetInterface(typeof(IQueryable).FullName!) != null)
          && name.StartsWith("OrderBy", StringComparison.Ordinal))
      {
        _orderingMethodFound = true;
      }

      return base.VisitMethodCall(node);
    }

    public static bool OrderMethodExists(Expression expression)
    {
      var visitor = new OrderingMethodFinder();
      visitor.Visit(expression);
      return visitor._orderingMethodFound;
    }
  }
}