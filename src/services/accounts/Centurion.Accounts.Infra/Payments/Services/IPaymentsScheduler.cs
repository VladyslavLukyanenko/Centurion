using CSharpFunctionalExtensions;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Security;

namespace Centurion.Accounts.Infra.Payments.Services;

public interface IPaymentsScheduler
{
  ValueTask<Result> ScheduleSalaryPayoutAsync(Dashboard dashboard, UserMemberRoleBinding binding,
    CancellationToken ct = default);

  ValueTask<Result> CancelSalaryPayoutAsync(Dashboard dashboard, UserMemberRoleBinding binding,
    CancellationToken ct = default);

  ValueTask<Result> ScheduleSalaryPayoutAsync(Dashboard dashboard, IEnumerable<UserMemberRoleBinding> bindings,
    CancellationToken ct = default);
}