namespace Centurion.Accounts.Core.Audit.Services;

public class MutableCurrentChangeSetProvider
  : IMutableChangeSetProvider
{
  public ChangeSet? CurrentChangSet { get; private set; }

  public void SetChangeSet(ChangeSet changeSet)
  {
    CurrentChangSet = changeSet;
  }
}