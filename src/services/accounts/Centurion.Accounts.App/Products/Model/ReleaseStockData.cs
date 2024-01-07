#pragma warning disable 8618
using System.Text.Json.Serialization;

namespace Centurion.Accounts.App.Products.Model;

public class ReleaseStockData
{
  public decimal Price { get; set; }
  public string Currency { get; set; }
  public bool IsLifetime { get; set; }
  public bool IsTrial { get; set; }
  public string LicenseDesc { get; set; }
  public Guid DashboardId { get; set; }
  [JsonIgnore]
  public int Stock { get; set; }
}