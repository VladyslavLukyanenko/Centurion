using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.Infra.Products.EfMappingConfigs;

public class EfDashboardBoundTypeConfigBase<T> : EfProductsMappingConfigBase<T>
  where T: class, IDashboardBoundEntity
{
  public override void Configure(EntityTypeBuilder<T> builder)
  {
    builder.HasOne<Dashboard>()
      .WithMany()
      .HasForeignKey(_ => _.DashboardId);

    base.Configure(builder);
  }
}