using CSharpFunctionalExtensions;
using Centurion.Accounts.Core.ChargeBackers;
using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.App.ChargeBackers.Services;

public interface IChargeBackerExportService
{
  ValueTask<Result> ExportAsync(ChargeBacker chargeBacker, Dashboard dashboard, CancellationToken ct = default);
}