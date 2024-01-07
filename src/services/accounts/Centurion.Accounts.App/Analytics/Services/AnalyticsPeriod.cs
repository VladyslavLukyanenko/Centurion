using CSharpFunctionalExtensions;
using NodaTime;
using NodaTime.Text;
using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.App.Analytics.Services;

public abstract class AnalyticsPeriod : Enumeration
{
  public static readonly AnalyticsPeriod Yearly = new YearlyAnalyticsPeriod(1, nameof(Yearly));
  public static readonly AnalyticsPeriod Monthly = new MonthlyAnalyticsPeriod(2, nameof(Monthly));

  private AnalyticsPeriod(int id, string name)
    : base(id, name)
  {
  }

  public abstract Result<(Instant StartOfPrev, Instant Start, Instant End)> GetTotalRange(string rawStart,
    Offset offset);

  public abstract int GetGroupingUnit(LocalDate date);
  public abstract bool IsPrevious(LocalDate start, LocalDate date);
  public abstract (int Curr, int Prev) GetDaysCount(LocalDate curr);

  private class YearlyAnalyticsPeriod : AnalyticsPeriod
  {
    public YearlyAnalyticsPeriod(int id, string name)
      : base(id, name)
    {
    }

    public override Result<(Instant StartOfPrev, Instant Start, Instant End)> GetTotalRange(string rawStart,
      Offset offset)
    {
      if (!int.TryParse(rawStart, out var year))
      {
        return Result.Failure<(Instant StartOfPrev, Instant Start, Instant End)>("Can't parse start year");
      }

      var startTime = new OffsetDateTime(new LocalDateTime(year, 1, 1, 00, 00), offset);
      var startOfPrevTime = startTime.With((LocalDate date) => date.Minus(Period.FromYears(1)));
      var endTime = new OffsetDateTime(new LocalDateTime(year, 12, 31, 23, 59, 59, 999), offset);

      return (startOfPrevTime.ToInstant(), startTime.ToInstant(), endTime.ToInstant());
    }

    public override int GetGroupingUnit(LocalDate date) => date.Month;
    public override bool IsPrevious(LocalDate start, LocalDate date) => start.Year < date.Year;

    public override (int Curr, int Prev) GetDaysCount(LocalDate curr)
    {
      var calendar = curr.Calendar;
      return (calendar.GetDaysInYear(curr.Year), calendar.GetDaysInYear(curr.Year - 1));
    }
  }

  private class MonthlyAnalyticsPeriod : AnalyticsPeriod
  {
    public MonthlyAnalyticsPeriod(int id, string name) : base(id, name)
    {
    }

    public override Result<(Instant StartOfPrev, Instant Start, Instant End)> GetTotalRange(string rawStart,
      Offset offset)
    {
      var parseResult = YearMonthPattern.Iso.Parse(rawStart);
      if (!parseResult.Success)
      {
        return Result.Failure<(Instant StartOfPrev, Instant Start, Instant End)>("Can't parse start month");
      }

      var startDate = parseResult.Value;
      var startMonth = startDate.Month;
      var startYear = startDate.Year;

      var startTime = new OffsetDateTime(new LocalDateTime(startYear, startMonth, 1, 00, 00), offset);
      var startOfPrevTime = startTime.With((LocalDate date) => date.Minus(Period.FromMonths(1)));

      var endDay = startTime.Calendar.GetDaysInMonth(startYear, startMonth);
      var endTime = new OffsetDateTime(new LocalDateTime(startYear, startMonth, endDay, 23, 59, 59, 999), offset);

      return (startOfPrevTime.ToInstant(), startTime.ToInstant(), endTime.ToInstant());
    }

    public override int GetGroupingUnit(LocalDate date) => date.Day;

    public override bool IsPrevious(LocalDate start, LocalDate date) => start.Year == date.Year
      ? start.Month < date.Month
      : start.Year < date.Year;

    public override (int Curr, int Prev) GetDaysCount(LocalDate curr)
    {
      var calendar = curr.Calendar;
      var prev = curr.Minus(Period.FromMonths(1));
      return (calendar.GetDaysInMonth(curr.Year, curr.Month), calendar.GetDaysInMonth(prev.Year, prev.Month));
    }
  }
}