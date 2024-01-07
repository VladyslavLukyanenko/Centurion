using CSharpFunctionalExtensions;
using Centurion.Accounts.Core.Forms;

namespace Centurion.Accounts.App.Forms.Services;

public interface IFormValueService
{
  ValueTask<Result> SubmitAsync(Form form, IEnumerable<FormFieldValue> values, long userId,
    CancellationToken ct = default);
}