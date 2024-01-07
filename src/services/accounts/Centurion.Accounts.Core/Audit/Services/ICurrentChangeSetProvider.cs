namespace Centurion.Accounts.Core.Audit.Services;

public interface ICurrentChangeSetProvider
{
  ChangeSet? CurrentChangSet { get; }
}