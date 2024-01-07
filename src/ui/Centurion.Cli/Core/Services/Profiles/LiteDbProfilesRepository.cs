using Centurion.Cli.Core.Domain.Profiles;
using LiteDB;

namespace Centurion.Cli.Core.Services.Profiles;

public class LiteDbProfilesRepository : LiteDbRepositoryBase<ProfileGroupModel>, IProfilesRepository
{
  public LiteDbProfilesRepository(ILiteDatabase liteDatabase)
    : base(liteDatabase)
  {
  }
}