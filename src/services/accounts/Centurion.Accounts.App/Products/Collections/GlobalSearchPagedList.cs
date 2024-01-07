using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.Core.Collections;

namespace Centurion.Accounts.App.Products.Collections;

public class GlobalSearchPagedList : PagedList<GlobalSearchResult>, IGlobalSearchPagedList
{
  public GlobalSearchPagedList(IEnumerable<GlobalSearchResult> data, int totalElements, PageRequest request,
    int licensesCount, int releasesCount)
    : base(data, totalElements, request)
  {
    LicensesCount = licensesCount;
    ReleasesCount = releasesCount;
  }

  public GlobalSearchPagedList(IEnumerable<GlobalSearchResult> data, int totalElements, int limit, int pageIndex,
    int licensesCount, int releasesCount)
    : base(data, totalElements, limit, pageIndex)
  {
    LicensesCount = licensesCount;
    ReleasesCount = releasesCount;
  }

  public int LicensesCount { get; }
  public int ReleasesCount { get; }
}