using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Centurion.Accounts.Core.ChargeBackers;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Infra.EfMappings;

namespace Centurion.Accounts.Infra.ChargeBackers.EfMappings;

public class ChargeBackerMappingConfig : EntityMappingConfig<ChargeBacker>
{
  protected override string SchemaName => "ChargeBackers";

  public override void Configure(EntityTypeBuilder<ChargeBacker> builder)
  {
    builder.HasOne<Dashboard>()
      .WithMany()
      .HasForeignKey(_ => _.DashboardId);

    builder.Property(_ => _.CardFingerprints)
      .UsePropertyAccessMode(PropertyAccessMode.Field)
      .HasColumnType("jsonb")
      .HasConversion(cards => ToJson(cards), json => FromJson<IReadOnlyList<string>>(json)!);

    base.Configure(builder);
  }
}