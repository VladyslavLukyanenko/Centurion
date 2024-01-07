using Centurion.Cli.Core.Domain;
using LiteDB;

namespace Centurion.Cli.Core.Services.Sessions;

public class LiteDbSessionRepository : LiteDbRepositoryBase<SessionModel, Guid>, ISessionRepository
{
  public LiteDbSessionRepository(ILiteDatabase database)
    : base(database)
  {
  }
}