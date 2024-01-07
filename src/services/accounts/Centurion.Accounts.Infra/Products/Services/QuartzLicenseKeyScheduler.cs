using System.Globalization;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;
using Centurion.Accounts.Infra.Products.Services.Jobs;
using Quartz;

namespace Centurion.Accounts.Infra.Products.Services;

public class QuartzLicenseKeyScheduler : ILicenseKeyScheduler
{
  private const string LicenseKeyGroup = "LicenseKeys";
  private readonly ISchedulerFactory _schedulerFactory;

  public QuartzLicenseKeyScheduler(ISchedulerFactory schedulerFactory)
  {
    _schedulerFactory = schedulerFactory;
  }

  public async ValueTask ScheduleKeyRemovalAsync(LicenseKey key, CancellationToken ct = default)
  {
    ITrigger trigger = TriggerBuilder.Create()
      .WithIdentity(key.Id.ToString(), LicenseKeyGroup)
      .StartAt(key.TrialEndsAt!.Value.ToDateTimeOffset())
      .Build();

    IJobDetail detail = JobBuilder.Create<RemoveLicenseKeyJob>()
      .WithIdentity(key.Id.ToString(), LicenseKeyGroup)
      .UsingJobData(nameof(RemoveLicenseKeyJob.LicenseKeyId), key.Id.ToString(CultureInfo.InvariantCulture))
      .UsingJobData(nameof(RemoveLicenseKeyJob.LicenseKeyValue), key.Value)
      .Build();

    var scheduler = await _schedulerFactory.GetScheduler(ct);
    await scheduler.ScheduleJob(detail, trigger, ct);
  }
}