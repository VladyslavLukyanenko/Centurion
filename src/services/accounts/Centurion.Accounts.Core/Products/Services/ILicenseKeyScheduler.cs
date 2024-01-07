namespace Centurion.Accounts.Core.Products.Services;

public interface ILicenseKeyScheduler
{
  ValueTask ScheduleKeyRemovalAsync(LicenseKey key, CancellationToken ct = default);
}