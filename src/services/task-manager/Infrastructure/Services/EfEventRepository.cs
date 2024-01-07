using Centurion.TaskManager.Core.Events;
using Microsoft.EntityFrameworkCore;

namespace Centurion.TaskManager.Infrastructure.Services;

public class EfEventRepository : EfCrudRepositoryBase<ProductCheckedOutEvent>, IEventRepository
{
  public EfEventRepository(DbContext ctx) : base(ctx)
  {
  }
}