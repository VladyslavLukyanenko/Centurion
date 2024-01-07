using Centurion.Accounts.Core.Events;

namespace Centurion.Accounts.Core.Products.Events.Dashboards;

public class ChargeBackersExportToggled : DomainEvent
{
  public ChargeBackersExportToggled(Guid dashboardId, bool isEnabled)
  {
    DashboardId = dashboardId;
    IsEnabled = isEnabled;
  }

  public Guid DashboardId { get; }
  public bool IsEnabled { get; }
}