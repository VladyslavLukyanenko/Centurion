using Centurion.Accounts.App.Config;
using Centurion.Accounts.App.Security.Model;
using Centurion.Accounts.App.Serialization.NewtonsoftJson.Converters;
using NodaTime;
using Centurion.Accounts.Core.Identity;

namespace Centurion.Accounts.App.Products.Model;

public class MemberSummaryData
{
  public long UserId { get; set; }
  public Guid DashboardId { get; set; }
  public string Name { get; set; } = null!;
  public string Discriminator { get; set; } = null!;
  public ulong DiscordId { get; set; }

  [UploadedFilePath(UserPicKeys.FallbackAvatarKey)]
  public string? Avatar { get; set; }

  public Instant JoinedAt { get; set; }

  public IEnumerable<DiscordRoleInfo> DiscordRoles { get; set; } = new List<DiscordRoleInfo>();
  public IList<BoundMemberRoleData> Roles { get; set; } = new List<BoundMemberRoleData>();
}