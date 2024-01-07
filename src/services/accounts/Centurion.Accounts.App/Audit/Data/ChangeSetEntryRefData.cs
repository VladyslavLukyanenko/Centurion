using NodaTime;
using Centurion.Accounts.Core.Audit;

namespace Centurion.Accounts.App.Audit.Data;

public class ChangeSetEntryRefData
{
  public Guid Id { get; set; }
  public Instant CreatedAt { get; set; }
  public string EntityId { get; set; } = null!;
  public string EntityType { get; set; } = null!;
  public ChangeType ChangeType { get; set; }
}