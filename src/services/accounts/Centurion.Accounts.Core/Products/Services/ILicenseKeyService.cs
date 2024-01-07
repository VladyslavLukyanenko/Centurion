using Centurion.Accounts.Core.Identity;
using CSharpFunctionalExtensions;

namespace Centurion.Accounts.Core.Products.Services;

public interface ILicenseKeyService
{
  ValueTask<Result<LicenseKey>> PurchaseAsync(User user, Plan plan, Release release, string keyValue,
    string? paymentIntent = null, string? subscriptionId = null, CancellationToken ct = default);

  ValueTask GenerateKeys(Plan plan, GenerateLicenseKeysCommand cmd, CancellationToken ct = default);

  ValueTask<Result> UnbindAsync(LicenseKey key, CancellationToken ct = default);
  ValueTask<Result> BindAsync(LicenseKey key, User user, string? discordToken, CancellationToken ct = default);
  ValueTask<Result> SuspendAsync(LicenseKey licenseKey, CancellationToken ct = default);
  ValueTask<Result> ResumeAsync(LicenseKey licenseKey, CancellationToken ct = default);
  ValueTask<Result> SuspendAllAsync(long planId, CancellationToken ct = default);
  ValueTask<Result> ResumeAllAsync(long planId, CancellationToken ct = default);
  ValueTask<Result> CancelSubscriptionAsync(string subscriptionId, CancellationToken ct = default);
  ValueTask<Result> RenewAsync(string subscriptionId, CancellationToken ct = default);
  ValueTask<Result> TryResumeSubscriptionAsync(string subscriptionId, CancellationToken ct = default);
  ValueTask<Result> UpdateAsync(LicenseKey licenseKey, UpdateLicenseKeyCommand cmd, CancellationToken ct = default);
  ValueTask<Result> RemoveKeyAsync(LicenseKey licenseKey, CancellationToken ct = default);
}