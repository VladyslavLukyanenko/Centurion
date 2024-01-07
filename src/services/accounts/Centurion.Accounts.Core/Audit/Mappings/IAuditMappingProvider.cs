namespace Centurion.Accounts.Core.Audit.Mappings;

public interface IAuditMappingProvider
{
  IDictionary<Type, IEntityMappingBuilder> Builders { get; }
}