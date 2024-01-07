namespace Centurion.Accounts.Core.Products.Services;

public interface IStatsService
{
  ValueTask RemoveByLicenseKeyIdAsync(long keyId, CancellationToken ct = default);
}

public class FakeStatsService : IStatsService
{
  public ValueTask RemoveByLicenseKeyIdAsync(long keyId, CancellationToken ct = default)
  {
    return default;
  }
}