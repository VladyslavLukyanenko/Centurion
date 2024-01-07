namespace Centurion.Accounts.App.Products.Model;

public class GlobalSearchResult
{
  public string Id { get; set; } = null!;
  public string Title { get; set; } = null!;
  public string? Details { get; set; } = null!;
  public SearchResultType Type { get; set; }
}