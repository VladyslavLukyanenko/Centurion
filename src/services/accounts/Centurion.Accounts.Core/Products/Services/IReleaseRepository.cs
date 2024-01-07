using Centurion.Accounts.Core.Primitives;
using CSharpFunctionalExtensions;

namespace Centurion.Accounts.Core.Products.Services;

public interface IReleaseRepository : ICrudRepository<Release>
{
  ValueTask<Release?> GetValidByPasswordAsync(string password, CancellationToken ct = default);
  ValueTask<Result> DecrementAsync(Release release, CancellationToken ct = default);
}