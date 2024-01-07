using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.Infra.Products.EfMappingConfigs;

public class EfDashboardMappingConfig : EfProductsMappingConfigBase<Dashboard>
{
  public override void Configure(EntityTypeBuilder<Dashboard> builder)
  {
    builder.HasOne<User>()
      .WithMany()
      .HasForeignKey(_ => _.OwnerId);

    builder.OwnsOne(_ => _.ProductInfo, pib =>
    {
      pib.Property(_ => _.Features)
        .HasConversion(f => ToJson(f), json => FromJson<IList<ProductFeature>>(json)!)
        .HasColumnType("jsonb");

      pib.Property(_ => _.Version).HasConversion(v => v.ToString(), s => Version.Parse(s));
    });
    builder.OwnsOne(_ => _.StripeConfig, sob =>
    {
      sob.Property(_ => _.ApiKey).IsRequired(false);
      sob.Property(_ => _.WebHookEndpointSecret).IsRequired(false);
    });
    builder.OwnsOne(_ => _.HostingConfig, hb =>
    {
      hb.HasIndex(_ => new {_.Mode, _.DomainName})
        .IsUnique();
    });
    builder.OwnsOne(_ => _.DiscordConfig, db => db.OwnsOne(_ => _.OAuthConfig));
    builder.Property(_ => _.ChargeBackersExportEnabled).UsePropertyAccessMode(PropertyAccessMode.Field);

    base.Configure(builder);
  }

  protected override void SetupIdGenerationStrategy(EntityTypeBuilder<Dashboard> builder)
  {
    // we don't want HiLo here
  }
}