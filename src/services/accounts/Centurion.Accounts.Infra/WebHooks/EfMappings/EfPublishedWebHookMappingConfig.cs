using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.WebHooks;

namespace Centurion.Accounts.Infra.WebHooks.EfMappings;

public class EfPublishedWebHookMappingConfig : WebHooksMappingConfigBase<PublishedWebHook>
{
  public override void Configure(EntityTypeBuilder<PublishedWebHook> builder)
  {
    builder.HasOne<Dashboard>()
      .WithMany()
      .HasForeignKey(_ => _.DashboardId);

    builder.OwnsOne(_ => _.Payload);
    base.Configure(builder);
  }
}