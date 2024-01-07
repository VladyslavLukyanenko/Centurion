using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.Forms.Services;

public interface IFormComponentRepository : ICrudRepository<FormComponent>
{
  ValueTask<IList<FormField>> GetFieldsAsync(long formId, CancellationToken ct = default);
}