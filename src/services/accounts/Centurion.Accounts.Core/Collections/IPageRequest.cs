namespace Centurion.Accounts.Core.Collections;

public interface IPageRequest
{
  int PageIndex { get; }
  string? OrderBy { get; }
  int Limit { get; }
  int Offset { get; }
  bool IsOrdered { get; }
}