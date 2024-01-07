
using Centurion.Accounts.Core.Collections;

namespace Centurion.Accounts.App.Model;

public class FilteredPageRequest : PageRequest, IFilteredPageRequest
{
  public FilteredPageRequest()
  {
  }

  public FilteredPageRequest(FilteredPageRequest request)
    : base(request)
  {
    SearchTerm = request.SearchTerm;
  }

  public string? SearchTerm { get; set; }

  public string NormalizeSearchTerm()
  {
    return SearchTerm?.ToUpperInvariant() ?? string.Empty;
  }

  public bool IsSearchTermEmpty()
  {
    return string.IsNullOrWhiteSpace(SearchTerm);
  }
}