using Centurion.Accounts.Infra.EfMappings;

namespace Centurion.Accounts.Infra.Products.EfMappingConfigs;

public abstract class EfProductsMappingConfigBase<T> : EntityMappingConfig<T>
  where T : class
{
  protected override string SchemaName => "Products";
}