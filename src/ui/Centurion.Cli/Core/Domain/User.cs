namespace Centurion.Cli.Core.Domain;

public class User
{
  public User(ulong id, string username, long discriminator, DateTimeOffset? licenseExpiryDate, string avatar)
  {
    Id = id;
    Username = username;
    Discriminator = discriminator;
    Expiry = licenseExpiryDate;
    Avatar = avatar;
    FullUserName = $"{username}#{discriminator.ToString().PadLeft(4, '0')}";
  }

  public DateTimeOffset? Expiry { get; private set; }
  public ulong Id { get; }
  public string Username { get; }
  public string FullUserName { get; set; }
  public long Discriminator { get; }
  public string Avatar { get; }
}