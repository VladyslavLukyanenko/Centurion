using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Core.Products.Services;
using Quartz;

namespace Centurion.Accounts.Infra.Products.Services.Jobs;

public class RemoveLicenseKeyJob : IJob
{
  private readonly ILicenseKeyRepository _licenseKeyRepository;
  private readonly IUnitOfWork _unitOfWork;

  public RemoveLicenseKeyJob(ILicenseKeyRepository licenseKeyRepository, IUnitOfWork unitOfWork)
  {
    _licenseKeyRepository = licenseKeyRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task Execute(IJobExecutionContext context)
  {
    try
    {
      var ct = context.CancellationToken;
      var licenseKey = await _licenseKeyRepository.GetByIdAsync(LicenseKeyId, ct);
      if (licenseKey == null || licenseKey.Value != LicenseKeyValue)
      {
        return;
      }

      _licenseKeyRepository.Remove(licenseKey);
      await _unitOfWork.SaveEntitiesAsync(ct);
    }
    catch (Exception exc)
    {
      throw new JobExecutionException(exc);
    }
  }

  public long LicenseKeyId { get; set; }
  public string LicenseKeyValue { get; set; } = null!;
}