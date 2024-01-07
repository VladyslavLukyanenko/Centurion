using Centurion.Accounts.Infra.EfMappings;

namespace Centurion.Accounts.Infra.Security.EfMappingConfigs;

public abstract class SecurityMappingConfigBase<T> : EntityMappingConfig<T>
  where T : class
{
  protected override string SchemaName => "Security";
}