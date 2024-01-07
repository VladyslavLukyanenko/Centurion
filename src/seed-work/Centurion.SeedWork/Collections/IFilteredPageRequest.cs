using Centurion.SeedWork.Collections;

// ReSharper disable once CheckNamespace
namespace Centurion.Accounts.App.Model;

public interface IFilteredPageRequest : IPageRequest
{
  string? SearchTerm { get; }
  string NormalizeSearchTerm();
  bool IsSearchTermEmpty();
}