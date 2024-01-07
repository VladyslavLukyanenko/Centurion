using Centurion.Cli.Core.Domain;
using LiteDB;

namespace Centurion.Cli.Core.Services.Harvesters;

public class LiteDbHarvestersRepository : LiteDbRepositoryBase<HarvesterModel>, IHarvestersRepository
{
  public LiteDbHarvestersRepository(ILiteDatabase database)
    : base(database)
  {
  }
}