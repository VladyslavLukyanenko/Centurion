using Centurion.Accounts.Core.Products.Services.LicenseKeyStrategies;

namespace Centurion.Accounts.Core.Products.Services;

public class IoCBasedLicenseKeyGenerationStrategyProvider : ILicenseKeyGenerationStrategyProvider
{
  private readonly IEnumerable<ILicenseKeyGenerationStrategy> _strategies;

  public IoCBasedLicenseKeyGenerationStrategyProvider(IEnumerable<ILicenseKeyGenerationStrategy> strategies)
  {
    _strategies = strategies;
  }

  public ILicenseKeyGenerationStrategy GetGeneratorFor(LicenseKeyGeneratorConfig config) =>
    _strategies.FirstOrDefault(_ => _.IsSupported(config))
    ?? throw new InvalidOperationException("Can't find license key generation strategy for format " + config.Format);
}