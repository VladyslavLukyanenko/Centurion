using Centurion.Contracts;
using Centurion.SeedWork.Infra.EfCoreNpgsql.Data.MappingConfigs;
using Centurion.TaskManager.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Centurion.TaskManager.Infrastructure.Data.MappingConfigs;

public class CheckoutTaskConfig : GenericEntityMappingConfig<CheckoutTask>
{
  public override void Configure(EntityTypeBuilder<CheckoutTask> builder)
  {
    builder.Property(_ => _.Module).HasConversion(e => e.ToString(), str => Enum.Parse<Module>(str));
    builder.Property(_ => _.ProfileIds)
      .HasConversion(set => ToJson(set), json => FromJson<HashSet<Guid>>(json) ?? new HashSet<Guid>())
      .HasColumnType("jsonb")
      .HasDefaultValueSql("'[]'");

    builder.HasOne<CheckoutTaskGroup>()
      .WithMany()
      .HasForeignKey(_ => _.GroupId);

    // builder.OwnsOne(_ => _.Variation);

    base.Configure(builder);
  }
}