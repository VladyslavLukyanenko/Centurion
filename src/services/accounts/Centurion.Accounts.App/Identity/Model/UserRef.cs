using Centurion.Accounts.App.Config;
using Centurion.Accounts.App.Serialization.NewtonsoftJson.Converters;

namespace Centurion.Accounts.App.Identity.Model;

public class UserRef
{
  public UserRef()
  {
  }

  public UserRef(long id)
  {
    Id = id;
  }

  public long Id { get; set; }
  public string FullName { get; set; } = null!;

  [UploadedFilePath(UserPicKeys.FallbackAvatarKey)]
  public string? Picture { get; set; }
}