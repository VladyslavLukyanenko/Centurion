using CSharpFunctionalExtensions;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.App.Products.Services;

public interface ILicenseKeyPaymentsService
{
  ValueTask<Result> ProcessPaymentAsync(Dashboard dashboard, long planId, Release release, string intent,
    string customer, User user,
    string discordToken, CancellationToken ct = default);

  ValueTask<Result> AcquireTrialKeyAsync(Dashboard dashboard, long planId, Release release, User user,
    string discordToken,
    CancellationToken ct = default);
}