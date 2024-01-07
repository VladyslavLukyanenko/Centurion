using System.Collections;

namespace Centurion.SeedWork.Collections;

public class PagedList<T>
  : IPagedList<T>
{
  public PagedList(IEnumerable<T> data, int totalElements, IPageRequest request)
    : this(data, totalElements, request.Limit, request.PageIndex)
  {
  }

  public PagedList(IEnumerable<T> data, int totalElements, int limit, int pageIndex)
  {
    Content = data.ToList();
    TotalElements = totalElements;
    TotalPages = (int)Math.Ceiling(totalElements / (double)limit);
    Limit = limit;
    PageIndex = pageIndex;
  }

  public bool IsEmpty => Count == 0;

  public bool IsFirst => PageIndex == 0;

  public bool IsLast => PageIndex >= TotalPages - 1;

  public int PageIndex { get; }

  public int Limit { get; }

  public int TotalElements { get; }

  public int TotalPages { get; }

  public IReadOnlyCollection<T> Content { get; }

  public IPagedList<TOther> CopyWith<TOther>(IEnumerable<TOther> content) =>
    new PagedList<TOther>(content, TotalElements, Limit, PageIndex);

  public IPagedList<TOther> ProjectTo<TOther>(Func<T, TOther> projection)
  {
    IEnumerable<TOther> projectedContent = Content.Select(projection);
    return CopyWith(projectedContent);
  }

  public int Count => Content.Count;

  public IEnumerable ToEnumerable() => Content;

  public static IPagedList<T> Empty(IPageRequest request) => Empty(request.Limit, request.PageIndex);

  private static IPagedList<T> Empty(int limit, int pageIndex) =>
    new PagedList<T>(Array.Empty<T>(), 0, limit, pageIndex);
}