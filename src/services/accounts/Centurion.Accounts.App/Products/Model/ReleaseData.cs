using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.App.Products.Model;

public class ReleaseData
{
  public long Id { get; set; }
  public long PlanId { get; set; }
    
  public string Title { get; set; } = null!;
  public int InitialStock { get; set; }
  public ReleaseType Type { get; set; }
  public bool IsActive { get; set; }
  public string Password { get; set; } = null!;
}