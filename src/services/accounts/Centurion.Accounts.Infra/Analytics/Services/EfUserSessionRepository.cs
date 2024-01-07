using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.Analytics;
using Centurion.Accounts.Core.Analytics.Services;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Infra.Repositories;

namespace Centurion.Accounts.Infra.Analytics.Services;

public class EfUserSessionRepository : EfCrudRepository<UserSession, Guid>, IUserSessionRepository
{
  public EfUserSessionRepository(DbContext context, IUnitOfWork unitOfWork)
    : base(context, unitOfWork)
  {
  }
}