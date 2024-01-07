using CSharpFunctionalExtensions;

namespace Centurion.Accounts.Core.Security.Services;

public interface IMemberRoleManager
{
  ValueTask<Result<MemberRole>> CreateAsync(Guid dashboardId, string name, IEnumerable<string> permissions,
    decimal? salary, PayoutFrequency? payoutFrequency, Currency? currency, string? colorHex,
    CancellationToken ct = default);
}