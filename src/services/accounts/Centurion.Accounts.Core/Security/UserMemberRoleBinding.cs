using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Security.Events;
using NodaTime;

namespace Centurion.Accounts.Core.Security;

public class UserMemberRoleBinding : AuditableEntity, IDashboardBoundEntity
{
  private UserMemberRoleBinding()
  {
  }

  public UserMemberRoleBinding(MemberRole role, User user)
  {
    MemberRoleId = role.Id;
    UserId = user.Id;
    DashboardId = role.DashboardId;

    AddDomainEvent(new UserMemberRoleGranted(user.Id, role.Id, role.Salary, role.DashboardId));
  }

  public long MemberRoleId { get; private set; }
  public long UserId { get; private set; }
  public Instant? LastPaidOutAt { get; private set; }
  public string? RemoteAccountId { get; set; }
  public Guid DashboardId { get; private set; }

  public Instant GetLastPaidOutAtOrDefault() => LastPaidOutAt.GetValueOrDefault(CreatedAt);
}