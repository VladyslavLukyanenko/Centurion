namespace Centurion.Accounts.Core.Audit.Processors;

public interface IAuditingEntityPreProcessor
{
  object PreProcess(object entity);
}