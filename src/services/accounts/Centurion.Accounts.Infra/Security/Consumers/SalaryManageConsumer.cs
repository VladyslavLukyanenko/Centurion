using MassTransit;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;
using Centurion.Accounts.Core.Security;
using Centurion.Accounts.Core.Security.Events;
using Centurion.Accounts.Core.Security.Services;
using Centurion.Accounts.Infra.Payments.Services;

namespace Centurion.Accounts.Infra.Security.Consumers;

public class SalaryManageConsumer : IConsumer<UserMemberRoleGranted>, IConsumer<UserMemberRoleRefused>,
  IConsumer<MemberRoleUpdated>
{
  private readonly IDashboardRepository _dashboardRepository;
  private readonly IPaymentsScheduler _paymentsScheduler;
  private readonly IPaymentsService _paymentsService;
  private readonly IUserMemberRoleBindingRepository _memberRoleBindingRepository;

  public SalaryManageConsumer(IDashboardRepository dashboardRepository, IPaymentsScheduler paymentsScheduler,
    IPaymentsService paymentsService, IUserMemberRoleBindingRepository memberRoleBindingRepository)
  {
    _dashboardRepository = dashboardRepository;
    _paymentsScheduler = paymentsScheduler;
    _paymentsService = paymentsService;
    _memberRoleBindingRepository = memberRoleBindingRepository;
  }

  public async Task Consume(ConsumeContext<UserMemberRoleGranted> context)
  {
    var m = context.Message;
    var ct = context.CancellationToken;
    if (!m.Salary.HasValue)
    {
      return;
    }

    await ExecuteOnBindingAsync(m.DashboardId, m.UserId, m.MemberRoleId,
      async (dashboard, binding) => await _paymentsScheduler.ScheduleSalaryPayoutAsync(dashboard, binding, ct), ct);
  }

  public async Task Consume(ConsumeContext<UserMemberRoleRefused> context)
  {
    var m = context.Message;
    var ct = context.CancellationToken;
    await ExecuteOnBindingAsync(m.DashboardId, m.UserId, m.MemberRoleId,
      async (dashboard, binding) =>
      {
        await _paymentsService.PayoutSalaryDueAsync(dashboard, binding, ct);
        await _paymentsScheduler.CancelSalaryPayoutAsync(dashboard, binding, ct);
      },
      ct);
  }

  private async ValueTask ExecuteOnBindingAsync(Guid dashboardId, long userId, long memberRoleId,
    Func<Dashboard, UserMemberRoleBinding, ValueTask> processor, CancellationToken ct)
  {
    var binding = await _memberRoleBindingRepository.GetUserRoleBindingAsync(userId, memberRoleId, ct);
    if (binding == null)
    {
      return;
    }

    var dashboard = await _dashboardRepository.GetByIdAsync(dashboardId, ct);
    await processor(dashboard!, binding);
  }

  public async Task Consume(ConsumeContext<MemberRoleUpdated> context)
  {
    var m = context.Message;
    if (m.OldSalary == m.NewSalary && m.OldFrequency == m.NewFrequency)
    {
      return;
    }

    var ct = context.CancellationToken;
    var dashboard = await _dashboardRepository.GetByIdAsync(m.DashboardId, ct);
    var bindings = await _memberRoleBindingRepository.GetBindingsAsync(m.DashboardId, m.MemberRoleId, ct);

    await _paymentsService.PayoutSalaryDueAsync(dashboard!, bindings, ct);
    await _paymentsScheduler.ScheduleSalaryPayoutAsync(dashboard!, bindings, ct);
  }
}