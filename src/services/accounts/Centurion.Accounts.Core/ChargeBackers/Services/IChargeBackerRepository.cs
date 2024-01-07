using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.ChargeBackers.Services;

public interface IChargeBackerRepository : ICrudRepository<ChargeBacker>
{
  ValueTask<IList<ChargeBacker>> GetNotExportedAsync(Guid dashboardId, CancellationToken ct = default);
}