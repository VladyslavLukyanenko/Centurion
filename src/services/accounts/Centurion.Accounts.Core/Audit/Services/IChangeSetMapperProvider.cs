using Centurion.Accounts.Core.Audit.Mappings;

namespace Centurion.Accounts.Core.Audit.Services;

public interface IChangeSetMapperProvider
{
  IEntityToChangeSetEntryMapper GetMapper(Type entityType);
  IEntityToChangeSetEntryMapper GetMapper(string entityType);
  bool HasMapperForType(Type type);
  bool HasMapperForType(string entityType);
}