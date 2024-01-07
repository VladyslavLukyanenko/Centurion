using Centurion.SeedWork.Primitives;

namespace Centurion.TaskManager.Core;

public class CheckoutTaskGroup : TimestampAuditableEntity<Guid>, IAuthorAuditable<string>, IUserProperty
{
  public string UserId { get; init; } = null!;
  public string Name { get; set; } = null!;
  public string? CreatedBy { get; private set; }
  public string? UpdatedBy { get; private set; }
}