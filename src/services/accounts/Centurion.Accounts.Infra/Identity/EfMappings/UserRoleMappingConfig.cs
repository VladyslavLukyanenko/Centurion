using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Centurion.Accounts.Core.Identity;

namespace Centurion.Accounts.Infra.Identity.EfMappings;

public class UserRoleMappingConfig : EfIdentityMappingConfigBase<UserRole>
{
  public override void Configure(EntityTypeBuilder<UserRole> builder)
  {
    builder.HasKey(r => new
    {
      r.UserId, r.RoleId
    });

    builder.HasOne<Role>(Infra.EfMappings.EntityMappingUtils.ResolveNavigationField<UserRole, Role>())
      .WithMany().HasForeignKey(ur => ur.RoleId).IsRequired();

    builder.HasOne<User>(Infra.EfMappings.EntityMappingUtils.ResolveNavigationField<UserRole, User>())
      .WithMany().HasForeignKey(ur => ur.UserId).IsRequired();
    base.Configure(builder);
  }
}