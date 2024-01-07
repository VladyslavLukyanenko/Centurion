using Centurion.Accounts.Core.Audit.Processors;
using Centurion.Accounts.Core.Audit.Services;

namespace Centurion.Accounts.Core.Audit.Mappings;

internal class GenericEntityToChangeSetEntryMapper
  : IEntityToChangeSetEntryMapper
{
  private readonly IDictionary<string, PropertyValueAccessor> _accessors;
  private readonly IDictionary<string, Type> _converters;
  private readonly PropertyValueAccessor _idAccessor;
  private readonly IAuditingEntityPreProcessor _preProcessor;
  private readonly IServiceProvider _serviceProvider;

  public GenericEntityToChangeSetEntryMapper(Type supportedClrType,
    IDictionary<string, PropertyValueAccessor> accessors,
    IDictionary<string, Type> converters, PropertyValueAccessor idAccessor,
    IAuditingEntityPreProcessor preProcessor, IServiceProvider serviceProvider)
  {
    _accessors = accessors;
    _converters = converters;
    _idAccessor = idAccessor;
    _preProcessor = preProcessor;
    _serviceProvider = serviceProvider;
    SupportedClrType = supportedClrType;
    SupportedType = supportedClrType.Name;
  }

  public Type SupportedClrType { get; }
  public string SupportedType { get; }

  public ChangeSetEntry Map(object entity, ChangeType changeType)
  {
    var processedEntity = _preProcessor.PreProcess(entity);
    var id = _idAccessor(processedEntity);

    var propValues = _accessors.ToDictionary(_ => _.Key, _ => _.Value(processedEntity));
    return new ChangeSetEntry(id!, SupportedType, changeType, propValues);
  }

  public async Task<Dictionary<string, string?>> ResolveMappedPropsAsync(ChangeSetEntry entry,
    CancellationToken ct = default)
  {
    var props = entry.Payload;
    foreach (var propName in props.Keys.ToArray())
    {
      if (_converters.TryGetValue(propName.ToLowerInvariant(), out var converterType))
      {
        var converter = (IChangeSetEntryValueConverter) _serviceProvider.GetService(converterType)!;
          
        props[propName] = await converter.ConvertAsync(props[propName], ct);
      }
    }

    return props;
  }
}