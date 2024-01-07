using Centurion.Contracts;
using Centurion.SeedWork.Infra.EfCoreNpgsql;
using Microsoft.EntityFrameworkCore;

namespace Centurion.WebhookSender.Infrastructure;

public class WebhooksContext : DbContext
{
  public WebhooksContext(DbContextOptions<WebhooksContext> o)
    : base(o)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<WebhookSettings>().HasKey(_ => _.UserId);

    base.OnModelCreating(modelBuilder);
    modelBuilder.UseSnakeCaseNamingConvention();
  }
}