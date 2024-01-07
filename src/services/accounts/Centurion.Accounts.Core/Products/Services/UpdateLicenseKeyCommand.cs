namespace Centurion.Accounts.Core.Products.Services;

public class UpdateLicenseKeyCommand
{
  public long Id { get; set; }
  public bool IsUnbindable { get; set; }
  public bool IsLifetime { get; set; }
  public long PlanId { get; set; }
  public string Notes { get; set; } = null!;
}