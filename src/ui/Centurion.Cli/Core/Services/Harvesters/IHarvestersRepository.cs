using Centurion.Cli.Core.Domain;

namespace Centurion.Cli.Core.Services.Harvesters;

public interface IHarvestersRepository : IRepository<HarvesterModel, Guid>
{
}