using Centurion.TaskManager.Core;
using Centurion.TaskManager.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Centurion.TaskManager.Infrastructure.Services;

public class EfCheckoutTaskGroupRepository : EfCrudRepositoryBase<CheckoutTaskGroup, Guid>, ICheckoutTaskGroupRepository
{
  public EfCheckoutTaskGroupRepository(DbContext ctx) : base(ctx)
  {
  }
}