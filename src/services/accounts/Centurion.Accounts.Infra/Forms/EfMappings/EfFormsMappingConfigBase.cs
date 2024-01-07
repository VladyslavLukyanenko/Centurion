using Centurion.Accounts.Infra.EfMappings;

namespace Centurion.Accounts.Infra.Forms.EfMappings;

public abstract class EfFormsMappingConfigBase<T> : EntityMappingConfig<T> where T : class
{
  protected override string SchemaName => "Forms";
}