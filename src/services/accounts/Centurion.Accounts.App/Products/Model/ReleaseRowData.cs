using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.App.Products.Model;

public class ReleaseRowData
{
  public long Id { get; set; }
  public string Title { get; set; } = null!;
  public int InitialStock { get; set; }
  public int Stock { get; set; }
  public ReleaseType Type { get; set; }
  public string PlanDesc { get; set; } = null!;
  public bool IsActive { get; set; }
}