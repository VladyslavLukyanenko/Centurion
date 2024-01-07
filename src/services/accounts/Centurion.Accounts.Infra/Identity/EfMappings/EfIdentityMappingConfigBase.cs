using Centurion.Accounts.Infra.EfMappings;

namespace Centurion.Accounts.Infra.Identity.EfMappings;

public abstract class EfIdentityMappingConfigBase<T> : EntityMappingConfig<T>
  where T : class
{
  protected override string SchemaName => "Identity";
}