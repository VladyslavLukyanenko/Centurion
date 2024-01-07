using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Core.WebHooks;
using Centurion.Accounts.Core.WebHooks.Services;
using Centurion.Accounts.Infra.Repositories;

namespace Centurion.Accounts.Infra.WebHooks;

public class EfPublishedWebHookRepository : EfCrudRepository<PublishedWebHook>, IPublishedWebHookRepository
{
  public EfPublishedWebHookRepository(DbContext context, IUnitOfWork unitOfWork)
    : base(context, unitOfWork)
  {
  }
}