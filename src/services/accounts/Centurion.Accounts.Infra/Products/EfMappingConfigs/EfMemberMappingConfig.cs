using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.Infra.Products.EfMappingConfigs;

public class EfMemberMappingConfig : EfProductsMappingConfigBase<Member>
{
  public override void Configure(EntityTypeBuilder<Member> builder)
  {
    builder.HasKey(_ => new {_.DashboardId, _.UserId});
    builder.HasOne<Dashboard>()
      .WithMany()
      .HasForeignKey(_ => _.DashboardId);
    builder.HasOne<User>()
      .WithMany()
      .HasForeignKey(_ => _.UserId);

    base.Configure(builder);
  }
}