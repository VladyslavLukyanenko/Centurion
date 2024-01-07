using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Centurion.Accounts.Core.Identity;

namespace Centurion.Accounts.Infra.Identity.EfMappings;

public class UserMappingConfig : EfIdentityMappingConfigBase<User>
{
  public override void Configure(EntityTypeBuilder<User> builder)
  {
    builder.Ignore(_ => _.IsLockedOut);

    builder.HasIndex(u => u.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique();
    builder.Property(u => u.UserName).HasMaxLength(256);
    builder.Property(u => u.NormalizedUserName).HasMaxLength(256);
    builder.Property(_ => _.PasswordHash).IsRequired(false);
    builder.Property(_ => _.StripeCustomerId).IsRequired(false);

    builder.Property(_ => _.DiscordRoles)
      .UsePropertyAccessMode(PropertyAccessMode.Field)
      .HasColumnType("jsonb")
      .HasConversion(roles => ToJson(roles), json => FromJson<List<DiscordRoleInfo>>(json)!);

    builder.OwnsOne(_ => _.Email, eb =>
    {
      eb.Property(u => u.Value).HasMaxLength(256).HasColumnName("Email");
      eb.HasIndex(u => u.NormalizedValue).HasDatabaseName("EmailIndex");
      eb.Property(u => u.NormalizedValue).HasMaxLength(256).HasColumnName("NormalizedEmail");
      eb.Property(_ => _.IsConfirmed).HasColumnName("IsEmailConfirmed");
    });


    builder.HasMany<UserClaim>().WithOne().HasForeignKey(uc => uc.UserId).IsRequired();
    base.Configure(builder);
  }
}