using Centurion.Cli.Core.Domain.Accounts;
using LiteDB;

namespace Centurion.Cli.Core.Services.Accounts;

public class AccountsRepository : LiteDbRepositoryBase<Account>, IAccountsRepository
{
  public AccountsRepository(ILiteDatabase database) : base(database)
  {
  }
}