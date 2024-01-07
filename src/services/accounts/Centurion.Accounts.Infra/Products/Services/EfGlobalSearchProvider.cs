using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.App.Products.Collections;
using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Infra.Services;

namespace Centurion.Accounts.Infra.Products.Services;

public class EfGlobalSearchProvider : DataProvider, IGlobalSearchProvider
{
  private readonly IQueryable<Release> _releases;
  private readonly IQueryable<LicenseKey> _licenseKeys;
  private readonly IQueryable<User> _users;

  public EfGlobalSearchProvider(DbContext context) : base(context)
  {
    _releases = GetAliveDataSource<Release>();
    _licenseKeys = GetAliveDataSource<LicenseKey>();
    _users = GetDataSource<User>();
  }

  public async ValueTask<IGlobalSearchPagedList> GetAllAsync(Guid dashboardId, GlobalSearchPageRequest pageRequest,
    CancellationToken ct = default)
  {
    var releases = _releases;
    var licenses = from l in _licenseKeys
      join u in _users on l.UserId equals u.Id into tmp
      from u in tmp.DefaultIfEmpty()
      join r in _releases on l.ReleaseId equals r.Id into rtmp
      from r in rtmp.DefaultIfEmpty()
      where l.DashboardId == dashboardId
      orderby l.UserId.HasValue descending, l.UpdatedAt descending, l.Value
      select new {License = l, User = u, Release = r};
    if (!pageRequest.IsSearchTermEmpty())
    {
      var normalizedSearch = $"%{pageRequest.SearchTerm}%";
      releases = releases.Where(_ => EF.Functions.ILike(_.Title, normalizedSearch));

      licenses = licenses.Where(_ => EF.Functions.ILike(_.License.Value, normalizedSearch)
                                     || _.Release != null && EF.Functions.ILike(_.Release.Title, normalizedSearch)
                                     || _.User != null && (
                                       EF.Functions.ILike(_.User.Name, normalizedSearch)
                                       || EF.Functions.ILike(_.User.Email.Value, normalizedSearch)
                                       || EF.Functions.ILike(_.User.Discriminator, normalizedSearch)
                                       || EF.Functions.ILike((string) (object) _.User.DiscordId, normalizedSearch)
                                     ));
    }

    var releasesQuery = from r in releases
      where r.DashboardId == dashboardId
      orderby r.CreatedAt descending
      select new GlobalSearchResult
      {
        Id = (string) (object) r.Id,
        Title = r.Title,
        Details = (string) (object) r.CreatedAt,
        Type = SearchResultType.Release
      };

    var licenseKeysQuery = from l in licenses
      select new GlobalSearchResult
      {
        Id = (string) (object) l.License.Id,
        Title = l.License.Value,
        Details = l.User == null ? null : l.User.Name + "#" + l.User.Discriminator,
        Type = SearchResultType.LicenseKey
      };


    var searchResults = releasesQuery.Union(licenseKeysQuery);
    var counts = await searchResults
      .GroupBy(_ => _.Type)
      .Select(r => new
      {
        Type = r.Key,
        Count = r.Count()
      })
      .ToDictionaryAsync(_ => _.Type, _ => _.Count, ct);
    var totalCount = counts.Values.Sum();
    if (totalCount == 0)
    {
      return new GlobalSearchPagedList(Array.Empty<GlobalSearchResult>(), 0, pageRequest, 0, 0);
    }

    var data = await searchResults
      .OrderByDescending(_ => _.Type == SearchResultType.Release)
      .Skip(pageRequest.Offset)
      .Take(pageRequest.Limit)
      .ToListAsync(ct);

    counts.TryGetValue(SearchResultType.LicenseKey, out var licensesCount);
    counts.TryGetValue(SearchResultType.Release, out var releasesCount);
    return new GlobalSearchPagedList(data, totalCount, pageRequest, licensesCount, releasesCount);
  }
}