using Centurion.Accounts.App.Forms.Model;

namespace Centurion.Accounts.App.Forms.Services;

public interface IFormProvider
{
  ValueTask<FormData?> GetUserFormAsync(long formId, long userId, CancellationToken ct = default);
}