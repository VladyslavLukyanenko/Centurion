using Centurion.Cli.Core.Domain.Profiles;

namespace Centurion.Cli.Core.Services.Profiles;

public interface IProfilesImportExportService : IImportExportService
{
  Task<bool> ImportFromJsonIntoGroupAsync(FileStream file, ProfileGroupModel selectedGroup,
    CancellationToken ct = default);
}