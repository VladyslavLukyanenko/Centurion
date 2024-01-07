using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Security;

namespace Centurion.Accounts.Infra.Security.EfMappingConfigs;

public class UserMemberRolesMappingConfig : SecurityMappingConfigBase<UserMemberRoleBinding>
{
  public override void Configure(EntityTypeBuilder<UserMemberRoleBinding> builder)
  {
    builder.HasOne<User>()
      .WithMany()
      .HasForeignKey(_ => _.UserId);

    builder.HasOne<MemberRole>()
      .WithMany()
      .HasForeignKey(_ => _.MemberRoleId);
      
    base.Configure(builder);
  }
}