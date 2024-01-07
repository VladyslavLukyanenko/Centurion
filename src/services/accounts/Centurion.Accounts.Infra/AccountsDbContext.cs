using Microsoft.EntityFrameworkCore;

namespace Centurion.Accounts.Infra;

public class AccountsDbContext : DbContext
{
  public AccountsDbContext(DbContextOptions<AccountsDbContext> options)
    : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    modelBuilder.UseSnakeCaseNamingConvention();
    base.OnModelCreating(modelBuilder);
  }
}