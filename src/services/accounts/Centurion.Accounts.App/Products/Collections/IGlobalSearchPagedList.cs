using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.Core.Collections;

namespace Centurion.Accounts.App.Products.Collections;

public interface IGlobalSearchPagedList : IPagedList<GlobalSearchResult>
{
  int LicensesCount { get; }
  int ReleasesCount { get; }
}