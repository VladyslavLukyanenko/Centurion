using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Centurion.Accounts.Core.Security;
using Centurion.Accounts.Infra.EfMappings;

namespace Centurion.Accounts.Infra.Security.EfMappingConfigs;

public class MemberRoleMappingConfig : SecurityMappingConfigBase<MemberRole>
{
  public override void Configure(EntityTypeBuilder<MemberRole> builder)
  {
    builder.Property(_ => _.Permissions)
      .UsePropertyAccessMode(PropertyAccessMode.Field)
      .HasColumnType("jsonb")
      .HasConversion(p => ToJson(p), json => FromJson<HashSet<string>>(json)!);

    builder.Property(_ => _.Name).IsRequired();
    builder.HasIndex(_ => new {_.Name, _.DashboardId})
      .IsUnique();

    builder.Property(_ => _.Currency).HasNullableEnumerationConversion().IsRequired(false);
    builder.Property(_ => _.PayoutFrequency).HasNullableEnumerationConversion().IsRequired(false);

    base.Configure(builder);
  }
}