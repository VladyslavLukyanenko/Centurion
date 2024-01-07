using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Centurion.Accounts.App.Analytics.Model;
using Centurion.Accounts.App.Analytics.Services;
using Centurion.Accounts.Core.Analytics;
using Centurion.Accounts.Core.Orders;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Services;
using Centurion.Accounts.Infra.Services;

namespace Centurion.Accounts.Infra.Analytics.Services;

public class EfAnalyticsProvider : DataProvider, IAnalyticsProvider
{
  private readonly IQueryable<PaymentTransaction> _transactions;
  private readonly IQueryable<Member> _members;
  private readonly IQueryable<LicenseKey> _licenseKeys;
  private readonly IQueryable<UserSession> _userSessions;

  private readonly IUserAgentService _userAgentService;

  public EfAnalyticsProvider(DbContext context, IUserAgentService userAgentService)
    : base(context)
  {
    _userAgentService = userAgentService;
    _transactions = GetDataSource<PaymentTransaction>();
    _members = GetDataSource<Member>();
    _licenseKeys = GetAliveDataSource<LicenseKey>();
    _userSessions = GetDataSource<UserSession>();
  }

  public async ValueTask<Result<GeneralAnalytics>> GetGeneralAnalyticsAsync(Guid dashboardId,
    GeneralAnalyticsRequest request, CancellationToken ct = default)
  {
    var period = Enumeration.FromDisplayName<AnalyticsPeriod>(request.Period);
    var offset = request.Offset;
    var totalResult = period.GetTotalRange(request.Start, offset);
    if (totalResult.IsFailure)
    {
      return totalResult.ConvertFailure<GeneralAnalytics>();
    }

    var (startOfPrev, startTime, endTime) = totalResult.Value;
    var startDate = startTime.WithOffset(offset).Date;

    var (incomes, keysSold, totalRevenue) = await AggregateIncomesAsync(dashboardId, startOfPrev, endTime, offset,
      startDate, period, ct);

    var (avgVisitorsStats, visitorsStats) = await AggregateVisitorsStatsAsync(dashboardId, startOfPrev, endTime,
      offset, period, startTime, startDate, ct);

    var analytics = new GeneralAnalytics
    {
      TotalUsers = await AggregateTotalUsersAsync(dashboardId, startOfPrev, startTime, endTime, ct),
      RetentionRate = await AggregateRetentionRateAsync(dashboardId, startTime, startOfPrev, endTime, ct),
      KeysSold = keysSold,
      TotalRevenue = totalRevenue,
      Income = incomes,
      AvgLiveViews = avgVisitorsStats,
      Visitors = visitorsStats
    };

    return analytics;
  }

  private async Task<(AverageVisitorsStats AvgVisitorsStats, VisitorsStats VisitorsStats)>
    AggregateVisitorsStatsAsync(Guid dashboardId, Instant startOfPrev, Instant endTime, Offset offset,
      AnalyticsPeriod period, Instant startTime, LocalDate startDate, CancellationToken ct)
  {
    var sessions = await _userSessions
      .Where(_ => _.StartedAt >= startOfPrev && _.StartedAt <= endTime && _.DashboardId == dashboardId)
      .Select(_ => new
      {
        _.UserId,
        _.Id,
        _.StartedAt,
        _.UserAgent,
        _.LastActivityAt
      })
      .ToArrayAsync(ct);

    int currDays = 0,
      prevDays = 0;

    var visitorsData = new Dictionary<int, int>();
    var avgVisitorsStats = new AverageVisitorsStats();
    foreach (var userSessions in sessions.GroupBy(_ => _.UserId?.ToString() ?? _.Id.ToString()))
    {
      var deviceTypes = userSessions.Select(session => _userAgentService.ResolveDeviceType(session.UserAgent))
        .ToArray();
      if (deviceTypes.Contains(UserAgentDeviceType.Desktop))
      {
        avgVisitorsStats.DesktopsCount++;
      }
      else if (deviceTypes.Contains(UserAgentDeviceType.Mobile))
      {
        avgVisitorsStats.MobileCount++;
      }

      var startedAt = userSessions.Min(_ => _.StartedAt);
      var visitStart = startedAt.WithOffset(offset)
        .LocalDateTime
        .Date;

      var lastActivityAt = userSessions.Max(_ => _.LastActivityAt);
      var visitEnd = lastActivityAt.WithOffset(offset)
        .LocalDateTime
        .Date;

      // var days = (visitEnd - visitStart).Days + 1;
      for (LocalDate i = visitStart; i <= visitEnd; i = i.PlusDays(1))
      {
        var unit = period.GetGroupingUnit(i);
        if (!visitorsData.TryGetValue(unit, out _))
        {
          visitorsData[unit] = 0;
        }

        visitorsData[unit]++;
      }

      if (startedAt >= startTime || lastActivityAt >= startTime)
      {
        // curr
        currDays++;
        // currDays += days;
      }

      if (startedAt >= startOfPrev && startedAt < startTime)
      {
        // prev
        // prevDays += days;
        prevDays++;
      }
    }

    var (currDaysCount, prevDaysCount) = period.GetDaysCount(startDate);
    var avgCurr = (int) Math.Ceiling((double) currDays / currDaysCount);
    var avgPrev = (int) Math.Ceiling((double) prevDays / prevDaysCount);
    avgVisitorsStats.LiveViewsCount = ValueDiff<int>.CreateInt32(avgCurr, avgPrev);

    var visitorsStats = new VisitorsStats
    {
      Count = ValueDiff<int>.CreateInt32(currDays, prevDays),
      Data = visitorsData.Select(p => new VisitorsStatsItem {Value = p.Value, GroupUnit = p.Key}).ToList()
    };

    var visitorsTuple = (avgVisitorsStats, visitorsStats);
    return visitorsTuple;
  }

  private async Task<ValueDiff<int>> AggregateTotalUsersAsync(Guid dashboardId, Instant startOfPrev,
    Instant startTime, Instant endTime, CancellationToken ct)
  {
    var users = await _members
      .Where(_ => _.DashboardId == dashboardId && _.JoinedAt >= startOfPrev && _.JoinedAt <= endTime)
      .Select(_ => _.JoinedAt)
      .ToArrayAsync(ct);
    var usersCount = users.Count(d => d >= startTime);
    var totalUsers = ValueDiff<int>.CreateInt32(usersCount, users.Length - usersCount);
    return totalUsers;
  }

  private async ValueTask<(List<IncomeStatsItem> Incomes, ValueDiff<int> KeysSold, ValueDiff<decimal> TotalRevenue)>
    AggregateIncomesAsync(Guid dashboardId, Instant startOfPrev, Instant endTime, Offset offset,
      LocalDate startDate, AnalyticsPeriod period, CancellationToken ct)
  {
    var sells = (await _transactions
        .Where(_ => _.DashboardId == dashboardId && _.CreatedAt >= startOfPrev && _.CreatedAt <= endTime
                    && _.ProductType == ProductTypes.LicenseKey)
        .Select(_ => new
        {
          _.Amount,
          _.CreatedAt
        })
        .ToArrayAsync(ct))
      .Select(_ => new
      {
        _.CreatedAt.WithOffset(offset).Date,
        _.Amount
      })
      .ToArray();


    var keysCount = sells.Count(_ => _.Date >= startDate);
    var revenue = sells.Where(_ => _.Date >= startDate).Sum(_ => _.Amount);

    var incomes = sells.GroupBy(_ => period.GetGroupingUnit(_.Date))
      .Select(i =>
      {
        var item = new IncomeStatsItem
        {
          GroupUnit = i.Key
        };

        foreach (var r in i)
        {
          if (period.IsPrevious(startDate, r.Date))
          {
            item.Amount.Previous += r.Amount;
          }
          else
          {
            item.Amount.Current += r.Amount;
          }
        }

        return item;
      })
      .ToList();
    var keysSold = ValueDiff<int>.CreateInt32(keysCount, sells.Length - keysCount);
    var totalRevenue = ValueDiff<decimal>.CreateDecimal(revenue, sells.Sum(_ => _.Amount) - revenue);

    return (incomes, keysSold, totalRevenue);
  }

  private async ValueTask<ValueDiff<float>> AggregateRetentionRateAsync(Guid dashboardId, Instant startTime,
    Instant startOfPrev, Instant endTime, CancellationToken ct)
  {
    var retentionRateKeys = await _licenseKeys.Where(_ =>
        _.SubscribedAt.HasValue
        && _.SubscribedAt <= endTime
        && _.Expiry >= startOfPrev
        && _.DashboardId == dashboardId)
      .Select(_ => new
      {
        SubscribedAt = _.SubscribedAt!.Value,
        _.SubscriptionCancelledAt
      })
      .ToArrayAsync(ct);

    int prevTotalKeys = 0, prevCancelled = 0, currTotalKeys = 0, currCancelled = 0;
    foreach (var key in retentionRateKeys)
    {
      if (key.SubscribedAt >= startTime)
      {
        // curr
        ++currTotalKeys;
        if (key.SubscriptionCancelledAt.HasValue)
        {
          ++currCancelled;
        }
      }
      else
      {
        // prev
        ++prevTotalKeys;
        if (key.SubscriptionCancelledAt.HasValue)
        {
          ++prevCancelled;
        }
      }
    }

    var retentionRate = ValueDiff<float>.CreateSingle(
      CalculateRetentionPercents(currTotalKeys, currCancelled),
      CalculateRetentionPercents(prevTotalKeys, prevCancelled));
    return retentionRate;

    float CalculateRetentionPercents(int total, int cancelled) =>
      total == 0
        ? 0
        : cancelled == 0
          ? 100
          : cancelled / (float) total * 100;
  }
}