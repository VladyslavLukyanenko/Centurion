using Centurion.Accounts.Core.Events;

namespace Centurion.Accounts.Core.Security.Events;

public class MemberRoleUpdated : DomainEvent
{
  public MemberRoleUpdated(long memberRoleId, decimal? oldSalary, decimal? newSalary,
    Guid dashboardId, PayoutFrequency oldFrequency, PayoutFrequency newFrequency)
  {
    MemberRoleId = memberRoleId;
    OldSalary = oldSalary;
    NewSalary = newSalary;
    DashboardId = dashboardId;
    OldFrequency = oldFrequency;
    NewFrequency = newFrequency;
  }

  public long MemberRoleId { get; }
  public decimal? OldSalary { get; }
  public decimal? NewSalary { get; }
  public Guid DashboardId { get; }
  public PayoutFrequency OldFrequency { get; }
  public PayoutFrequency NewFrequency { get; }
}