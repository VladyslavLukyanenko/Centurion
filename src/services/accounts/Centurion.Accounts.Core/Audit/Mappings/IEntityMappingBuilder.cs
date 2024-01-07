namespace Centurion.Accounts.Core.Audit.Mappings;

public interface IEntityMappingBuilder
{
  AuditEntityMapping Mapping { get; }
  Type MappedType { get; }
  Type? PreProcessorType { get; }
}