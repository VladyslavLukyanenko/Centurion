using Centurion.CloudManager.Domain;
using Centurion.SeedWork.Infra.EfCoreNpgsql.Data.MappingConfigs;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Centurion.CloudManager.Infra.EfMappings;

public class NodeSnapshotEfMapping : GenericEntityMappingConfig<NodeSnapshot>
{
  public override void Configure(EntityTypeBuilder<NodeSnapshot> builder)
  {
    builder.OwnsOne(_ => _.User, _ =>
    {
      _.HasIndex(_ => _.Id).IsUnique();
    });

    base.Configure(builder);
  }
}