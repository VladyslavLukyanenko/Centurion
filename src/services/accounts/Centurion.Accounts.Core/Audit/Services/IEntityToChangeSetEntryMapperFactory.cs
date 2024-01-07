using Centurion.Accounts.Core.Audit.Mappings;

namespace Centurion.Accounts.Core.Audit.Services;

public interface IEntityToChangeSetEntryMapperFactory
{
  IEntityToChangeSetEntryMapper Create(IEntityMappingBuilder builder);
}