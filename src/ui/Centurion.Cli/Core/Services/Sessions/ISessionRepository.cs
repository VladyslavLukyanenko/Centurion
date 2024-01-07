using Centurion.Cli.Core.Domain;

namespace Centurion.Cli.Core.Services.Sessions;

public interface ISessionRepository : IRepository<SessionModel, Guid>
{
}