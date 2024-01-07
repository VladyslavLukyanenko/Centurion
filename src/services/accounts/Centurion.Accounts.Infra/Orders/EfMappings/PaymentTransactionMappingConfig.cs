using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Orders;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Infra.EfMappings;

namespace Centurion.Accounts.Infra.Orders.EfMappings;

public class PaymentTransactionMappingConfig : EntityMappingConfig<PaymentTransaction>
{
  protected override string SchemaName => "Orders";

  public override void Configure(EntityTypeBuilder<PaymentTransaction> builder)
  {
    builder.HasOne<User>()
      .WithMany()
      .HasForeignKey(_ => _.UserId);

    builder.HasOne<Dashboard>()
      .WithMany()
      .HasForeignKey(_ => _.DashboardId);

    base.Configure(builder);
  }

  protected override void SetupIdGenerationStrategy(EntityTypeBuilder<PaymentTransaction> builder)
  {
    // no need HiLo here
  }
}