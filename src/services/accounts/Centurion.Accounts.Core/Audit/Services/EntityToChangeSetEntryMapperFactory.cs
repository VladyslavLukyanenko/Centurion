using Centurion.Accounts.Core.Audit.Mappings;
using Centurion.Accounts.Core.Audit.Processors;

namespace Centurion.Accounts.Core.Audit.Services;

public class EntityToChangeSetEntryMapperFactory : IEntityToChangeSetEntryMapperFactory
{
  private readonly IServiceProvider _serviceProvider;

  public EntityToChangeSetEntryMapperFactory(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
  }

  public IEntityToChangeSetEntryMapper Create(IEntityMappingBuilder builder)
  {
    var preProcessor =
      (IAuditingEntityPreProcessor) _serviceProvider.GetService(
        builder.PreProcessorType ?? typeof(NullAuditEntityPreProcessor))!;
    return new GenericEntityToChangeSetEntryMapper(builder.MappedType, builder.Mapping.Accessors,
      builder.Mapping.Converters, builder.Mapping.IdAccessor, preProcessor, _serviceProvider);
  }
}