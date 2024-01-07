using Centurion.Contracts;
using Centurion.SeedWork.Infra.EfCoreNpgsql.Data.MappingConfigs;
using Centurion.TaskManager.Core.Presets;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Centurion.TaskManager.Infrastructure.Data.MappingConfigs;

public class PresetConfig : GenericEntityMappingConfig<Preset>
{
  public override void Configure(EntityTypeBuilder<Preset> builder)
  {
    builder.Property(_ => _.Module).HasConversion(e => e.ToString(), str => Enum.Parse<Module>(str));
    base.Configure(builder);
  }
}