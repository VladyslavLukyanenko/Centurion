namespace Centurion.Accounts.Core.Products.Services;

public class GenerateLicenseKeysCommand
{
  public bool IsLifetime { get; set; }
  public long PlanId { get; set; }
  public uint TrialDaysCount { get; set; }
  public uint Quantity { get; set; }
  public bool AllowUnbinding { get; set; }
}