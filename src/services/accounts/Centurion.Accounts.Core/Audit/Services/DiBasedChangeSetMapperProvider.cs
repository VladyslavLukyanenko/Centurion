using Centurion.Accounts.Core.Audit.Mappings;

namespace Centurion.Accounts.Core.Audit.Services;

public class DiBasedChangeSetMapperProvider
  : IChangeSetMapperProvider
{
  private readonly IDictionary<Type, IEntityToChangeSetEntryMapper> _mappersByClrType =
    new Dictionary<Type, IEntityToChangeSetEntryMapper>();

  private readonly IDictionary<string, IEntityToChangeSetEntryMapper> _mappersByType =
    new Dictionary<string, IEntityToChangeSetEntryMapper>();

  public DiBasedChangeSetMapperProvider(IEnumerable<IEntityToChangeSetEntryMapper> mappers)
  {
    foreach (var mapper in mappers)
    {
      _mappersByClrType[mapper.SupportedClrType] = mapper;
      _mappersByType[mapper.SupportedType] = mapper;
    }
  }

  public IEntityToChangeSetEntryMapper GetMapper(Type entityType)
  {
    return _mappersByClrType[entityType];
  }

  public IEntityToChangeSetEntryMapper GetMapper(string entityType)
  {
    return _mappersByType[entityType];
  }

  public bool HasMapperForType(Type type)
  {
    return _mappersByClrType.ContainsKey(type);
  }

  public bool HasMapperForType(string entityType)
  {
    return _mappersByType.ContainsKey(entityType);
  }
}