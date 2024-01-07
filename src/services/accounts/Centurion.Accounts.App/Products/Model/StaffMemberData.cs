using Centurion.Accounts.App.Config;
using Centurion.Accounts.App.Serialization.NewtonsoftJson.Converters;

namespace Centurion.Accounts.App.Products.Model;

public class StaffMemberData
{
  /// <summary>
  /// User id
  /// </summary>
  public long Id { get; set; }

  public string Discriminator { get; set; } = null!;

  [UploadedFilePath(UserPicKeys.FallbackAvatarKey)]
  public string? Avatar { get; set; }

  public string Name { get; set; } = null!;
}