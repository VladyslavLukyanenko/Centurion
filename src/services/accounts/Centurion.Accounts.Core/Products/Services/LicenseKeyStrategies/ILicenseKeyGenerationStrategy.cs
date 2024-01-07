namespace Centurion.Accounts.Core.Products.Services.LicenseKeyStrategies;

public interface ILicenseKeyGenerationStrategy
{
  bool IsSupported(LicenseKeyGeneratorConfig config);
  ValueTask<string> GenerateValueAsync(Plan plan, CancellationToken ct = default);
}