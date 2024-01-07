using Centurion.Cli.Core.Domain.Profiles;

namespace Centurion.Cli.Core.Services.Profiles;

public interface IProfilesRepository : IRepository<ProfileGroupModel, Guid>
{
}