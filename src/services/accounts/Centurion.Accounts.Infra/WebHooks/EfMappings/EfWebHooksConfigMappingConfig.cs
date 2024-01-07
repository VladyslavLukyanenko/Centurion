using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.WebHooks;

namespace Centurion.Accounts.Infra.WebHooks.EfMappings;

public class EfWebHooksConfigMappingConfig : WebHooksMappingConfigBase<WebHooksConfig>
{
  public override void Configure(EntityTypeBuilder<WebHooksConfig> builder)
  {
    builder.HasOne<Dashboard>()
      .WithMany()
      .HasForeignKey(_ => _.DashboardId);

    base.Configure(builder);
  }
}