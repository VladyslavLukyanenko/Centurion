using Centurion.Cli.Core.Domain.Accounts;
using Centurion.Cli.Core.Domain.Fields;
using Centurion.Cli.Core.Domain.Profiles;

namespace Centurion.Cli.Core.Services.Accounts;

public interface IAccountGenerator
{
  IEnumerable<Field> ConfigurationFields { get; }
  IAsyncEnumerable<GeneratedAccount> GenerateAsync(CancellationToken ct = default);
  Task InitializeAsync(Func<ProfileModel> profileProvider, CancellationToken ct = default);
  Task PrepareAsync(CancellationToken ct = default);
}