using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.App.Identity.Model;
using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Infra.Services;

namespace Centurion.Accounts.Infra.Products.Services;

public class EfUserProvider : DataProvider, IUserProvider
{
  private readonly IMapper _mapper;
  private readonly IQueryable<User> _users;

  public EfUserProvider(DbContext dbContext, IMapper mapper) : base(dbContext)
  {
    _mapper = mapper;
    _users = GetDataSource<User>();
  }
  public async Task<UserData?> GetUserByIdAsync(long id, CancellationToken ct = default)
  {
    var user = await _users.FirstOrDefaultAsync(_ => _.Id == id, ct);
    return _mapper.Map<UserData>(user);
  }
}