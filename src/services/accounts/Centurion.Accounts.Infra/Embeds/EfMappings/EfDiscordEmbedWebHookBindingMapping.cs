using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Centurion.Accounts.Core.Embeds;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Infra.EfMappings;

namespace Centurion.Accounts.Infra.Embeds.EfMappings;

public class EfDiscordEmbedWebHookBindingMapping : EntityMappingConfig<DiscordEmbedWebHookBinding>
{
  protected override string SchemaName => "Embeds";

  public override void Configure(EntityTypeBuilder<DiscordEmbedWebHookBinding> builder)
  {
    builder.Property(_ => _.MessageTemplate)
      .HasColumnType("jsonb")
      .HasConversion(t => ToJson(t), json => FromJson<EmbedMessageTemplate>(json)!);

    builder.HasOne<Dashboard>()
      .WithMany()
      .HasForeignKey(_ => _.DashboardId);

    builder.Property(_ => _.EventType).IsRequired();
    builder.Property(_ => _.WebhookUrl).IsRequired();
      
    base.Configure(builder);
  }
}