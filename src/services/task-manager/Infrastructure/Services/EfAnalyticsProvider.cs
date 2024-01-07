using AutoMapper;
using Centurion.Contracts.Analytics;
using Centurion.SeedWork.Collections;
using Centurion.SeedWork.Infra.EfCoreNpgsql;
using Centurion.TaskManager.Application.Services.Analytics;
using Centurion.TaskManager.Core.Events;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Text;

namespace Centurion.TaskManager.Infrastructure.Services;

public class EfAnalyticsProvider : IAnalyticsProvider
{
  private readonly DbContext _context;
  private readonly IMapper _mapper;

  public EfAnalyticsProvider(DbContext context, IMapper mapper)
  {
    _context = context;
    _mapper = mapper;
  }

  public async ValueTask<IPagedList<CheckoutInfoData>> GetCheckoutsPage(string userId, ICheckoutInfoPageRequest request,
    CancellationToken ct = default)
  {
    var query = _context.Set<ProductCheckedOutEvent>()
      .Where(_ => _.UserId == userId)
      .Where(_ => _.Timestamp >= request.StartAt && _.Timestamp <= request.EndAt);

    if (!request.IsSearchTermEmpty())
    {
      var q = request.NormalizeSearchTerm();
      query = query.Where(_ =>
        _.Store.Contains(q)
        || _.Account!.Contains(q)
        || _.Sku.Contains(q)
        || _.Profile.Contains(q)
        || _.Title.Contains(q));
    }

    var page = await query
      .OrderByDescending(_ => _.Timestamp)
      .AsNoTracking()
      .PaginateAsync(request, ct);

    return page.ProjectTo(_mapper.Map<CheckoutInfoData>);
  }

  public async ValueTask<AnalyticsSummary> GetSummary(string userId, DateTimeZone timeZone,
    CancellationToken ct = default)
  {
    var nowAtMidnight = SystemClock.Instance.GetCurrentInstant()
      .InZone(timeZone)
      .ToOffsetDateTime()
      .With(_ => LocalTime.Midnight);

    var endOfDay = (nowAtMidnight.Plus(Duration.FromDays(1)) - Duration.FromMilliseconds(1))
      .ToInstant();

    var startTime = nowAtMidnight.ToInstant();
    var startMonth = nowAtMidnight.With(DateAdjusters.StartOfMonth).ToInstant();
    var endMonth = nowAtMidnight.With(DateAdjusters.EndOfMonth).ToInstant();
    var general = await _context.Set<ProductCheckedOutEvent>()
      .Where(_ => _.UserId == userId)
      .Where(_ => _.Timestamp >= startTime && _.Timestamp <= endOfDay)
      .GroupBy(_ => _.UserId)
      .Select(g => new { Checkouts = g.Count(), Spent = g.Sum(_ => _.Price) })
      .FirstOrDefaultAsync(ct) ?? new { Checkouts = 0, Spent = 0M };

    var entries = (await _context.Set<ProductCheckedOutEvent>()
        .Where(_ => _.UserId == userId)
        .Where(_ => _.Timestamp >= startMonth && _.Timestamp <= endMonth)
        .Select(e => new { e.Price, e.Timestamp })
        .ToArrayAsync(ct))
      .GroupBy(e => e.Timestamp.InZone(timeZone).Date)
      .OrderBy(_ => _.Key)
      .Select(g => new CheckoutEntry
      {
        Count = (uint)g.Count(),
        TotalPrice = (double)g.Sum(_ => _.Price),
        Date = LocalDatePattern.Iso.Format(g.Key)
      });

    return new AnalyticsSummary
    {
      Entries = { entries },
      TodayStats = new TodayStats
      {
        CheckoutsCount = ((uint)general.Checkouts),
        SpentTotal = ((double)general.Spent)
      }
    };
  }
}