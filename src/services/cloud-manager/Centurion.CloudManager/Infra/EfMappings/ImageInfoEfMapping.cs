using Centurion.CloudManager.Domain;
using Centurion.SeedWork.Infra.EfCoreNpgsql.Data.MappingConfigs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Centurion.CloudManager.Infra.EfMappings;

public class ImageInfoEfMapping : GenericEntityMappingConfig<ImageInfo>
{
  public override void Configure(EntityTypeBuilder<ImageInfo> builder)
  {
    builder.Property(_ => _.RequiredSpawnParameters)
      .HasConversion(l => ToJson(l), json => FromJson<HashSet<string>>(json)!)
      .HasColumnType("jsonb");
    
    base.Configure(builder);
  }
}