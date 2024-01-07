using Centurion.Accounts.Core.Products;
using NodaTime;
using NodaTime.TimeZones;

namespace Centurion.Accounts.Core.Security;

public class MemberRole : DashboardBoundEntity
{
  private HashSet<string> _permissions = new();

  private MemberRole()
  {
  }

  public MemberRole(Guid dashboardId, string name, IEnumerable<string> permissions, decimal? salary,
    PayoutFrequency? payoutFrequency, Currency? currency, string? colorHex)
    : base(dashboardId)
  {
    Name = name;
    Salary = salary;
    PayoutFrequency = payoutFrequency;
    Currency = currency;
    ColorHex = colorHex;
    _permissions = new HashSet<string>(permissions);
  }

  public string Name { get; private set; } = null!;
  public IReadOnlyCollection<string> Permissions => _permissions;
  public decimal? Salary { get; private set; }
  public Currency? Currency { get; private set; }
  public PayoutFrequency? PayoutFrequency { get; private set; }
  public string? ColorHex { get; private set; }

  public decimal CalculateDueAmount(UserMemberRoleBinding binding, Dashboard dashboard)
  {
    if (!Salary.HasValue)
    {
      throw new CoreException("Role has no configured salary");
    }

    var timeSpentPercents = CalculateTimeSpentPercent(binding, dashboard);
    if (timeSpentPercents == 0)
    {
      return 0;
    }

    return Salary.Value * timeSpentPercents / 100;
  }

  private int CalculateTimeSpentPercent(UserMemberRoleBinding binding, Dashboard dashboard)
  {
    var lastPaidOutAt = binding.GetLastPaidOutAtOrDefault();
    var timeZone = BclDateTimeZone.FromTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById(dashboard.TimeZoneId));
    var lastPayoutTime = lastPaidOutAt.InZone(timeZone);
    var now = SystemClock.Instance
      .GetCurrentInstant()
      .InZone(timeZone);

    var daysInMonth = CalendarSystem.Iso.GetDaysInMonth(now.Year, now.Month);
    if (lastPayoutTime.Month != now.Month)
    {
      // full month. to pay 100% of salary for each
      var fullMonthsCount = (now.Month - lastPayoutTime.Month - 1) * 100;
      var totalDaysForLast = CalendarSystem.Iso.GetDaysInMonth(lastPayoutTime.Year, lastPayoutTime.Month);

      var percentsForLast = CalculateForDay(totalDaysForLast, totalDaysForLast, lastPayoutTime.Day);
      var percentsForThisMonth = CalculateForDay(daysInMonth, now.Day, 1);

      return percentsForLast + percentsForThisMonth + fullMonthsCount;
    }

    return CalculateForDay(daysInMonth, now.Day, lastPayoutTime.Day);
  }

  private static int CalculateForDay(int totalDays, int today, int startingDay) =>
    (int) Math.Floor(totalDays / 100D * (today - 1 - startingDay));

  public bool HasConfiguredPayout() =>
    Salary.HasValue && PayoutFrequency != null && Currency != null;

  public void ChangePermissions(IEnumerable<string> permissions)
  {
    _permissions.Clear();
    _permissions.UnionWith(permissions);
  }
}