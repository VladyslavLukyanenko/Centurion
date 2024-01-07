namespace Centurion.Accounts.Core.Products.Services.LicenseKeyStrategies;

public class CustomLicenseKeyGenerationStrategy : ILicenseKeyGenerationStrategy
{
  private readonly Lazy<ILicenseKeyGenerationStrategyProvider> _strategyProvider;

  public CustomLicenseKeyGenerationStrategy(Lazy<ILicenseKeyGenerationStrategyProvider> strategyProvider)
  {
    _strategyProvider = strategyProvider;
  }

  public bool IsSupported(LicenseKeyGeneratorConfig config) => config.Format == LicenseKeyFormat.Custom;

  public async ValueTask<string> GenerateValueAsync(Plan plan, CancellationToken ct = default)
  {
    var cfg = plan.LicenseKeyConfig;
    var key = cfg.Template!.Replace("{PlanId}", plan.Id.ToString(), StringComparison.OrdinalIgnoreCase)
      .Replace("{DashboardId}", plan.DashboardId.ToString(), StringComparison.OrdinalIgnoreCase);

    const string guidPlaceholder = "{Guid}";
    const string rndStringPlaceholder = "{RandomString}";
    if (key.Contains(guidPlaceholder, StringComparison.OrdinalIgnoreCase))
    {
      var guidStrategy = _strategyProvider.Value.GetGeneratorFor(LicenseKeyGeneratorConfig.Guid());
      var guid = await guidStrategy.GenerateValueAsync(plan, ct);
      key = key.Replace(guidPlaceholder, guid, StringComparison.OrdinalIgnoreCase);
    }

    if (key.Contains(rndStringPlaceholder, StringComparison.OrdinalIgnoreCase))
    {
      var guidStrategy = _strategyProvider.Value.GetGeneratorFor(LicenseKeyGeneratorConfig.RandomString());
      var rndStr = await guidStrategy.GenerateValueAsync(plan, ct);
      key = key.Replace(rndStringPlaceholder, rndStr, StringComparison.OrdinalIgnoreCase);
    }

    return key;
  }
}