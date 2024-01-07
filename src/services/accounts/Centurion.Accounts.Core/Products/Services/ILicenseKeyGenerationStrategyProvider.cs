using Centurion.Accounts.Core.Products.Services.LicenseKeyStrategies;

namespace Centurion.Accounts.Core.Products.Services;

public interface ILicenseKeyGenerationStrategyProvider
{
  ILicenseKeyGenerationStrategy GetGeneratorFor(LicenseKeyGeneratorConfig config);
}