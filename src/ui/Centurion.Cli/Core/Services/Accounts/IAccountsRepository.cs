using Centurion.Cli.Core.Domain.Accounts;

namespace Centurion.Cli.Core.Services.Accounts;

public interface IAccountsRepository : IRepository<Account, Guid>
{
}