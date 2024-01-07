namespace Centurion.Accounts.Core.Audit.Services;

public interface IMutableChangeSetProvider
  : ICurrentChangeSetProvider
{
  void SetChangeSet(ChangeSet changeSet);
}