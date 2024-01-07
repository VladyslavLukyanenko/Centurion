using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.App.Identity.Model;
using Centurion.Accounts.App.Identity.Services;
using Centurion.Accounts.Core.Collections;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Infra.Services;

namespace Centurion.Accounts.Infra.Identity.Providers;

public class UserRefProvider : DataProvider, IUserRefProvider
{
  private readonly IMapper _mapper;

  private readonly IQueryable<User> _aliveUsers;

  public UserRefProvider(DbContext context, IMapper mapper)
    : base(context)
  {
    _mapper = mapper;
    _aliveUsers = GetAliveDataSource<User>();
  }

  public async ValueTask<UserRef> GetRefAsync(long userId, CancellationToken ct = default)
  {
    return await _aliveUsers.Where(_ => _.Id == userId)
      .ProjectTo<UserRef>(_mapper.ConfigurationProvider)
      .FirstOrDefaultAsync(ct);
  }

  public async ValueTask<IDictionary<long, UserRef>> GetRefsAsync(IEnumerable<long> userIds,
    CancellationToken ct = default)
  {
    userIds = userIds.Distinct();
    var refs = await _aliveUsers.Where(_ => userIds.Contains(_.Id))
      .ProjectTo<UserRef>(_mapper.ConfigurationProvider)
      .ToDictionaryAsync(_ => _.Id, ct);

    return refs;
  }

  public async ValueTask<IPagedList<UserRef>> GetRefsPageAsync(UserRefsPageRequest pageRequest, CancellationToken ct)
  {
    var query = _aliveUsers;
    if (!pageRequest.IsSearchTermEmpty())
    {
      var normalizedSearchTerm = pageRequest.NormalizeSearchTerm();

      query = query.Where(_ =>
        _.Email.NormalizedValue.Contains(normalizedSearchTerm)
        || _.UserName.Contains(normalizedSearchTerm)
        || _.Name.Contains(normalizedSearchTerm));
    }

    if (pageRequest.ExcludedIds.Any())
    {
      query = query.Where(_ => !pageRequest.ExcludedIds.Contains(_.Id));
    }

    if (pageRequest.SelectedId.HasValue)
    {
      query = query.OrderByDescending(_ => _.Id == pageRequest.SelectedId);
    }

    var page = await query.ProjectTo<UserRef>(_mapper.ConfigurationProvider)
      .PaginateAsync(pageRequest, ct);

    return page;
  }
}