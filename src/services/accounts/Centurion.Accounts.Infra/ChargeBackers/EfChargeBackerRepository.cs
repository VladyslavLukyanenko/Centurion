using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.ChargeBackers;
using Centurion.Accounts.Core.ChargeBackers.Services;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Infra.Repositories;

namespace Centurion.Accounts.Infra.ChargeBackers;

public class EfChargeBackerRepository : EfCrudRepository<ChargeBacker>, IChargeBackerRepository
{
  public EfChargeBackerRepository(DbContext context, IUnitOfWork unitOfWork)
    : base(context, unitOfWork)
  {
  }

  public ValueTask<IList<ChargeBacker>> GetNotExportedAsync(Guid dashboardId, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }
}