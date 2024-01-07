using Centurion.Accounts.Infra.EfMappings;

namespace Centurion.Accounts.Infra.WebHooks.EfMappings;

public abstract class WebHooksMappingConfigBase<T> : EntityMappingConfig<T> where T : class
{
  protected override string SchemaName => "WebHooks";
}