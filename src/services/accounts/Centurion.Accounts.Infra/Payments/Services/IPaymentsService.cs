using CSharpFunctionalExtensions;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Security;

namespace Centurion.Accounts.Infra.Payments.Services;

public interface IPaymentsService
{
  ValueTask<Result> PayoutSalaryDueAsync(Dashboard dashboard, UserMemberRoleBinding binding,
    CancellationToken ct = default);

  ValueTask<Result> PayoutSalaryDueAsync(Dashboard dashboard, IEnumerable<UserMemberRoleBinding> bindings,
    CancellationToken ct = default);
}