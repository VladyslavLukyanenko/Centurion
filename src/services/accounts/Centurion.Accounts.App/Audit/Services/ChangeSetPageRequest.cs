using NodaTime;
using Centurion.Accounts.Core.Audit;
using Centurion.Accounts.Core.Collections;

namespace Centurion.Accounts.App.Audit.Services;

public class ChangeSetPageRequest : PageRequest
{
  public long? FacilityId { get; set; }
  public long? UserId { get; set; }
  public ChangeType? ChangeType { get; set; } = null!;

  public Instant From { get; set; }
  public Instant To { get; set; }
}