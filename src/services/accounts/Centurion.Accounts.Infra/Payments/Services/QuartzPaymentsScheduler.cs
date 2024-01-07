using CSharpFunctionalExtensions;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Security;
using Centurion.Accounts.Core.Security.Services;
using Centurion.Accounts.Infra.Payments.Jobs;
using Quartz;

namespace Centurion.Accounts.Infra.Payments.Services;

public class QuartzPaymentsScheduler : IPaymentsScheduler
{
  private const string PayoutsGroup = "payouts";
  private readonly ISchedulerFactory _schedulerFactory;
  private readonly IMemberRoleRepository _memberRoleRepository;

  public QuartzPaymentsScheduler(ISchedulerFactory schedulerFactory, IMemberRoleRepository memberRoleRepository)
  {
    _schedulerFactory = schedulerFactory;
    _memberRoleRepository = memberRoleRepository;
  }

  public async ValueTask<Result> ScheduleSalaryPayoutAsync(Dashboard dashboard, UserMemberRoleBinding binding,
    CancellationToken ct = default) =>
    await ScheduleSalaryPayoutAsync(dashboard, new[] {binding}, ct);

  public async ValueTask<Result> CancelSalaryPayoutAsync(Dashboard dashboard, UserMemberRoleBinding binding,
    CancellationToken ct = default)
  {
    var identity = ConstructJobIdentity(binding.MemberRoleId, binding.UserId);
    var jobKey = new JobKey(identity, PayoutsGroup);

    var scheduler = await _schedulerFactory.GetScheduler(ct);
    if (await scheduler.CheckExists(jobKey, ct))
    {
      await scheduler.DeleteJob(jobKey, ct);
    }

    return Result.Success();
  }

  public async ValueTask<Result> ScheduleSalaryPayoutAsync(Dashboard dashboard,
    IEnumerable<UserMemberRoleBinding> bindings, CancellationToken ct = default)
  {
    var roleBindings = bindings as UserMemberRoleBinding[] ?? bindings.ToArray();
    var roleIds = roleBindings.Select(_ => _.MemberRoleId);
    var roles = await _memberRoleRepository.GetByIdsAsync(roleIds, ct);

    MemberRole? role = null;
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

      var timeZone = TimeZoneInfo.FindSystemTimeZoneById(dashboard.TimeZoneId);
      var jobIdentity = ConstructJobIdentity(binding.MemberRoleId, binding.UserId);
      ITrigger trigger = TriggerBuilder.Create()
        .WithIdentity(jobIdentity, PayoutsGroup)
        .StartNow()
        .WithCronSchedule(role.PayoutFrequency!.CronPattern, _ => _.InTimeZone(timeZone))
        .Build();

      IJobDetail job = JobBuilder.Create<PayoutJob>()
        .WithIdentity(jobIdentity, PayoutsGroup)
        .UsingJobData(nameof(PayoutJob.DashboardId), dashboard.Id.ToString())
        .UsingJobData(nameof(PayoutJob.BindingId), binding.Id.ToString())
        .Build();

      var scheduler = await _schedulerFactory.GetScheduler(ct);
      await scheduler.ScheduleJob(job, trigger, ct);
    }

    return Result.Success();
  }

  private static string ConstructJobIdentity(long memberRoleId, long userId)
  {
    return $"{memberRoleId}__{userId}";
  }
}