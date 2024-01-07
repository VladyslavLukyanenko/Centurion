using Microsoft.EntityFrameworkCore;

namespace Centurion.Accounts.Infra.Audit;

public interface IDbContextChangesAuditor
{
  void AuditChanges(DbContext ctx);
}