using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.Products;

public abstract class DashboardBoundEntity : SoftRemovableEntity, IDashboardBoundEntity
{
  protected DashboardBoundEntity()
  {
  }

  protected DashboardBoundEntity(Guid dashboardId)
  {
    DashboardId = dashboardId;
  }

  public Guid DashboardId { get; private set; }
}