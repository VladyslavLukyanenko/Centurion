using Centurion.SeedWork.Infra.EfCoreNpgsql.Data.MappingConfigs;
using Centurion.TaskManager.Core.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Centurion.TaskManager.Infrastructure.Data.MappingConfigs;

public class CheckedOutProductEventConfig : GenericEntityMappingConfig<ProductCheckedOutEvent>
{
  protected override string SchemaName => "events";

  public override void Configure(EntityTypeBuilder<ProductCheckedOutEvent> builder)
  {
    builder.Property(_ => _.Attrs)
      .HasConversion(val => ToJson(val), json => FromJson<List<ProductAttr>>(json)!)
      .HasColumnType("jsonb");

    builder.Property(_ => _.Links)
      .HasConversion(val => ToJson(val), json => FromJson<List<ProductLink>>(json)!)
      .HasColumnType("jsonb");

    builder.Property(_ => _.ProcessingLog)
      .HasConversion(val => ToJson(val), json => FromJson<List<string>>(json)!)
      .HasColumnType("jsonb");

    base.Configure(builder);
  }
}