namespace Centurion.SeedWork.Infra.EfCoreNpgsql.Data.MappingConfigs;

public abstract class GenericEntityMappingConfig<T> : EntityMappingConfig<T>
  where T : class
{
  protected override string SchemaName => "public";
}