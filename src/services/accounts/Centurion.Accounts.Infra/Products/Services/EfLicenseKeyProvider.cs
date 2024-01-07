using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Centurion.Accounts.App.Identity.Model;
using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.Core.Collections;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Infra.Services;

namespace Centurion.Accounts.Infra.Products.Services;

public class EfLicenseKeyProvider : DataProvider, ILicenseKeyProvider
{
  private readonly IQueryable<LicenseKey> _aliveLicenseKeys;
  private readonly IQueryable<User> _users;
  private readonly IQueryable<Plan> _plans;
  private readonly IQueryable<Release> _releases;
  private readonly IQueryable<Member> _members;
  private readonly IMapper _mapper;

  public EfLicenseKeyProvider(DbContext context, IMapper mapper)
    : base(context)
  {
    _aliveLicenseKeys = GetAliveDataSource<LicenseKey>();
    _users = GetDataSource<User>();
    _plans = GetDataSource<Plan>();
    _releases = GetDataSource<Release>();
    _members = GetDataSource<Member>();
    _mapper = mapper;
  }

  public async ValueTask<IPagedList<LicenseKeyShortData>> GetPageByReleaseIdAsync(Guid dashboardId, long releaseId,
    PageRequest pageRequest, CancellationToken ct = default)
  {
    var query = from l in _aliveLicenseKeys
      join u in _users on l.UserId equals u.Id into utmp
      from u in utmp.DefaultIfEmpty()
      where l.ReleaseId == releaseId && l.DashboardId == dashboardId
      orderby l.CreatedAt descending, l.Id
      select new LicenseKeyShortData
      {
        Id = l.Id,
        Value = l.Value,
        User = u == null
          ? null
          : new UserRef
          {
            Id = u.Id,
            Picture = u.Avatar,
            FullName = u.Name + "#" + u.Discriminator
          }
      };


    return await query.PaginateAsync(pageRequest, ct);
  }

  public async ValueTask<IPagedList<LicenseKeySnapshotData>> GetPageAsync(Guid dashboardId,
    LicenseKeyPageRequest pageRequest, CancellationToken ct = default)
  {
    var searchTerm = pageRequest.NormalizeSearchTerm();
    // var allKeys = GetDataSource<LicenseKey>();

    var query = from item in CreateDenormalizedLicenseKeysQuery(dashboardId)
      where !pageRequest.LifetimeOnly.HasValue || item.Key.Expiry.HasValue != pageRequest.LifetimeOnly
        && (
          item.User!.Email.Value.Contains(searchTerm)
          || item.User.DiscordId.ToString().Contains(searchTerm)
          || item.Key.Value.Contains(searchTerm)
        )
      select item;

    if (pageRequest.PlanId.HasValue)
    {
      query = query.Where(_ => _.Plan.Id == pageRequest.PlanId);
    }

    if (pageRequest.ReleaseId.HasValue)
    {
      query = query.Where(_ => _.Release != null && _.Release.Id == pageRequest.ReleaseId);
    }

    if (pageRequest.SortBy.HasValue)
    {
      switch (pageRequest.SortBy.Value)
      {
        case LicensesSortBy.Newest:
          query = query.OrderByDescending(_ => _.Key.CreatedAt);
          break;
        case LicensesSortBy.Oldest:
          query = query.OrderBy(_ => _.Key.CreatedAt);
          break;
        case LicensesSortBy.Expiry:
          query = query.Where(_ => _.Key.Expiry.HasValue).OrderByDescending(_ => _.Key.Expiry);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    return await query.ProjectTo<LicenseKeySnapshotData>(_mapper.ConfigurationProvider)
      .PaginateAsync(pageRequest, ct);
  }

  private IQueryable<DenormalizedLicenseKey> CreateDenormalizedLicenseKeysQuery(Guid dashboardId)
  {
    var query = from licenseKey in _aliveLicenseKeys
      join user in _users on licenseKey.UserId equals user.Id
        into utmp
      from user in utmp.DefaultIfEmpty()
      join plan in _plans on licenseKey.PlanId equals plan.Id
      join release in _releases on licenseKey.ReleaseId equals release.Id
        into tmp
      from release in tmp.DefaultIfEmpty()
      orderby licenseKey.RemovedAt == Instant.MaxValue descending, licenseKey.CreatedAt, licenseKey.Value
      where licenseKey.DashboardId == dashboardId
      select new DenormalizedLicenseKey
      {
        Key = licenseKey,
        Plan = plan,
        User = user,
        Release = release
      };
    return query;
  }

  public async ValueTask<IList<PurchasedLicenseKeyData>> GetPurchasedKeysAsync(Guid dashboardId, long userId,
    CancellationToken ct = default)
  {
    return await CreateDenormalizedLicenseKeysQuery(dashboardId)
      .Where(_ => _.User != null && _.User.Id == userId)
      .OrderByDescending(_ => _.Key.UpdatedAt)
      .ProjectTo<PurchasedLicenseKeyData>(_mapper.ConfigurationProvider)
      .ToListAsync(ct);
  }

  public async ValueTask<LicenseKeySummaryData> GetSummaryByIdAsync(Guid dashboardId, long id,
    CancellationToken ct = default)
  {
    var query = from l in _aliveLicenseKeys
      join u in _users on l.UserId equals u.Id into utmp
      from u in utmp.DefaultIfEmpty()
      join jd in _members
        on new {UserId = l.UserId!.Value, l.DashboardId} equals new {jd.UserId, jd.DashboardId}
        into jtmp
      from jd in jtmp.DefaultIfEmpty()
      where l.DashboardId == dashboardId
      select new LicenseKeySummaryData
      {
        Id = l.Id,
        Expiry = l.Expiry,
        Reason = l.Reason,
        Value = l.Value,
        PlanId = l.PlanId,
        UnbindableAfter = l.UnbindableAfter,
        TrialEndsAt = l.TrialEndsAt,
        User = u == null
          ? null
          : new LicenseKeySummaryData.KeyOwnerSummaryData
          {
            Id = u.Id,
            Picture = u.Avatar,
            FullName = u.Name + "#" + u.Discriminator,
            JoinedAt = jd.JoinedAt
          }
      };

    return await query.FirstOrDefaultAsync(_ => _.Id == id, ct);
  }

  public async ValueTask<int> GetUsedTodayCountAsync(Guid dashboardId, Offset offset, CancellationToken ct = default)
  {
    var now = SystemClock.Instance.GetCurrentInstant()
      .WithOffset(offset);

    var from = now.With(_ => LocalTime.Midnight).ToInstant();
    var until = now.With(_ => LocalTime.Noon.PlusMilliseconds(-1)).ToInstant();

    return await _aliveLicenseKeys.CountAsync(_ =>
      _.DashboardId == dashboardId
      && _.LastAuthRequest.HasValue && _.LastAuthRequest.Value >= from && _.LastAuthRequest.Value <= until, ct);
  }
}

public class DenormalizedLicenseKey
{
  public LicenseKey Key { get; set; } = null!;
  public Plan Plan { get; set; } = null!;
  public Release? Release { get; set; }
  public User? User { get; set; }
}