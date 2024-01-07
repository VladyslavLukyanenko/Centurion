using CSharpFunctionalExtensions;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Security;
using Centurion.Accounts.Core.Security.Services;
using Centurion.Accounts.Infra.Services;
using Stripe;

namespace Centurion.Accounts.Infra.Payments.Services;

public class StripePaymentsService : IPaymentsService
{
  private readonly IMemberRoleRepository _memberRoleRepository;
  private readonly IStripeClientFactory _stripeClientFactory;

  public StripePaymentsService(IMemberRoleRepository memberRoleRepository, IStripeClientFactory stripeClientFactory)
  {
    _memberRoleRepository = memberRoleRepository;
    _stripeClientFactory = stripeClientFactory;
  }

  public async ValueTask<Result> PayoutSalaryDueAsync(Dashboard dashboard, UserMemberRoleBinding binding,
    CancellationToken ct = default) =>
    await PayoutSalaryDueAsync(dashboard, new[] {binding}, ct);

  public async ValueTask<Result> PayoutSalaryDueAsync(Dashboard dashboard,
    IEnumerable<UserMemberRoleBinding> bindings, CancellationToken ct = default)
  {
    var roleBindings = bindings as UserMemberRoleBinding[] ?? bindings.ToArray();
    var roleIds = roleBindings.Select(_ => _.MemberRoleId);
    var roles = await _memberRoleRepository.GetByIdsAsync(roleIds, ct);

    var transferService = new TransferService(_stripeClientFactory.CreateClient(dashboard));
    MemberRole? role = null;
    Result result = Result.Success();
    foreach (var binding in roleBindings)
    {
      if (role?.Id != binding.MemberRoleId)
      {
        role = roles.First(_ => binding.MemberRoleId == _.Id);
      }

      if (!role.HasConfiguredPayout())
      {
        continue;
      }

      var dueAmount = role.CalculateDueAmount(binding, dashboard);
      if (dueAmount == 0)
      {
        continue;
      }

      var options = new TransferCreateOptions
      {
        Amount = (long) Math.Round(dueAmount * 100),
        Currency = role.Currency!.Name,
        Destination = binding.RemoteAccountId
      };

      try
      {
        await transferService.CreateAsync(options, cancellationToken: ct);
      }
      catch (StripeException exc)
      {
        result = Result.Combine(result, Result.Failure(exc.StripeError.Message));
      }
    }

    return Result.Success();
  }
}