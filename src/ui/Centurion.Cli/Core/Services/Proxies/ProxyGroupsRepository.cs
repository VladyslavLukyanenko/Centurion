using Centurion.Cli.Core.Domain;
using LiteDB;

namespace Centurion.Cli.Core.Services.Proxies;

public class ProxyGroupsRepository : LiteDbRepositoryBase<ProxyGroup>, IProxyGroupsRepository
{
  public ProxyGroupsRepository(ILiteDatabase database) : base(database)
  {
  }
}