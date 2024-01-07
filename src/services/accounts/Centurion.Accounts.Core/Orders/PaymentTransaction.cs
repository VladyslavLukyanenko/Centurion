using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.Orders;

public class PaymentTransaction : AuditableEntity<Guid>
{
  public string ProductType { get; private set; } = null!;
  public decimal Amount { get; private set; }
  public long UserId { get; private set; }
  public Guid? DashboardId { get; private set; }
  public string SourceTxId { get; private set; } = null!;
}

public static partial class ProductTypes
{
  public const string LicenseKey = nameof(LicenseKey);
}