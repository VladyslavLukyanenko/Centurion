using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.Infra.Products.EfMappingConfigs;

internal class EfPlanMappingConfigs : EfDashboardBoundTypeConfigBase<Plan>
{
  public override void Configure(EntityTypeBuilder<Plan> builder)
  {
    // builder.HasOne<Product>()
    //   .WithMany()
    //   .HasForeignKey(_ => _.ProductId);

    builder.OwnsOne(_ => _.LicenseKeyConfig);

    base.Configure(builder);
  }
}