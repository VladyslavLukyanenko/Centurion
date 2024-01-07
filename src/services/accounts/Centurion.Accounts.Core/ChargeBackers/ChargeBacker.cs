using CSharpFunctionalExtensions;
using NodaTime;

namespace Centurion.Accounts.Core.ChargeBackers;

public class ChargeBacker : Primitives.Entity
{
  private List<string> _cardFingerprints = new();

  private ChargeBacker(Guid dashboardId)
  {
    DashboardId = dashboardId;
  }

  public ChargeBacker(string reason, string ipAddress, string email, Guid dashboardId)
  {
    Reason = reason;
    IpAddress = ipAddress;
    Email = email;
    DashboardId = dashboardId;
  }

  public Instant? ExportedAt { get; private set; }
  public string Reason { get; private set; } = null!;
  public string IpAddress { get; private set; } = null!;
  public string Email { get; private set; } = null!;
  public Guid DashboardId { get; private set; }
  public IReadOnlyList<string> CardFingerprints => _cardFingerprints.AsReadOnly();

  public void AddCards(IEnumerable<string> cardFingerprints)
  {
    _cardFingerprints.AddRange(cardFingerprints);
  }

  public Result Exported()
  {
    if (ExportedAt.HasValue)
    {
      return Result.Failure("Already exported");
    }

    ExportedAt = SystemClock.Instance.GetCurrentInstant();
    return Result.Success();
  }
}