using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.Forms.Services;

public interface IFormResponseRepository : ICrudRepository<FormResponse>
{
  // ValueTask<bool> AlreadyRespondedAsync(long formId, long userId, CancellationToken ct = default);
  ValueTask<IList<FormResponse>> GetByFormIdAsync(long formId, long userId, CancellationToken ct = default);
}