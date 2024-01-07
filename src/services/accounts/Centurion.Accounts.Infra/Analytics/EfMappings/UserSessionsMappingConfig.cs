using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Centurion.Accounts.Core.Analytics;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Infra.EfMappings;

namespace Centurion.Accounts.Infra.Analytics.EfMappings;

public class UserSessionsMappingConfig : EntityMappingConfig<UserSession>
{
  protected override string SchemaName => "Analytics";
  public override void Configure(EntityTypeBuilder<UserSession> builder)
  {
    builder.HasOne<User>()
      .WithMany()
      .HasForeignKey(_ => _.UserId);

    builder.HasOne<Dashboard>()
      .WithMany()
      .HasForeignKey(_ => _.DashboardId);

    base.Configure(builder);
  }

  protected override void SetupIdGenerationStrategy(EntityTypeBuilder<UserSession> builder)
  {
    // no need HiLo here
  }
}