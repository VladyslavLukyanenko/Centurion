using Centurion.Contracts;
using Centurion.SeedWork.Primitives;

namespace Centurion.TaskManager.Core;

public abstract class CheckoutTaskState : TimestampAuditableEntity<Guid>, IAuthorAuditable<string>
{
  public Module Module { get; init; }
  public string ProductSku { get; init; } = null!;
  public byte[] Config { get; init; } = null!;
  public string? CreatedBy { get; private set; }
  public string? UpdatedBy { get; private set; }
}