using Centurion.Contracts;
using Centurion.SeedWork.Infra.EfCoreNpgsql.Data.MappingConfigs;
using Centurion.TaskManager.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Centurion.TaskManager.Infrastructure.Data.MappingConfigs;

public class ProductConfig : GenericEntityMappingConfig<Product>
{
  public override void Configure(EntityTypeBuilder<Product> builder)
  {
    builder.Property(_ => _.Module).HasConversion(e => e.ToString(), str => Enum.Parse<Module>(str));

    builder.HasKey(_ => new { _.Module, _.Sku });
    base.Configure(builder);
  }
}