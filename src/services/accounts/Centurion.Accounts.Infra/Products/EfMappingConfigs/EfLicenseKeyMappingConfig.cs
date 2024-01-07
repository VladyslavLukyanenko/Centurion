using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.Infra.Products.EfMappingConfigs;

public class EfLicenseKeyMappingConfig : EfDashboardBoundTypeConfigBase<LicenseKey>
{
  public override void Configure(EntityTypeBuilder<LicenseKey> builder)
  {
    // builder.HasOne<Product>()
    //   .WithMany()
    //   .HasForeignKey(_ => _.ProductId);

    builder.HasOne<Release>()
      .WithMany()
      .HasForeignKey(_ => _.ReleaseId);

    builder.HasOne<Plan>()
      .WithMany()
      .HasForeignKey(_ => _.PlanId);

    builder.HasOne<User>()
      .WithMany()
      .HasForeignKey(_ => _.UserId);

    builder.Property(_ => _.Value).IsRequired();

    builder.Property(_ => _.Suspensions)
      .HasConversion(s => ToJson(s), json => FromJson<List<SuspensionLicensePeriod>>(json)!)
      .HasColumnType("jsonb")
      .UsePropertyAccessMode(PropertyAccessMode.Field);

    base.Configure(builder);
  }
}