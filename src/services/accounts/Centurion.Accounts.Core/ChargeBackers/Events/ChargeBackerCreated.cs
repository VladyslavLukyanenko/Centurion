using Centurion.Accounts.Core.Events;

namespace Centurion.Accounts.Core.ChargeBackers.Events;

public class ChargeBackerCreated : DomainEvent
{
  public long ChargeBackerId { get; private set; }
  public Guid DashboardId { get; private set; }
}