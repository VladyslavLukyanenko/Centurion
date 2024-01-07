using Centurion.Accounts.Core.Events;
using Centurion.Accounts.Core.Primitives;
using NodaTime;

namespace Centurion.Accounts.Core.Identity;

public class User : SoftRemovableEntity, IConcurrentEntity
{
  private List<DiscordRoleInfo> _discordRoles = new();
  private static readonly Duration ProfileValidityPeriod = Duration.FromMinutes(30);
  private DateTimeOffset? _lockoutEnd;

  private User()
  {
  }

  public static string? GetAvatarUrl(ulong userId, string? avatar) =>
    string.IsNullOrEmpty(avatar)
      ? null
      : $"https://cdn.discordapp.com/avatars/{userId}/{avatar}.png";

  public static User CreateWithDiscordId(string rawEmail, string name, ulong discordId, string? avatarId,
    string discriminator)
  {
    var email = Email.CreateConfirmed(rawEmail);
    var user = new User
    {
      Email = email,
      UserName = email.Value,
      DiscordId = discordId
    };

    user.UpdateProfile(name, GetAvatarUrl(discordId, avatarId), discriminator);

    return user;
  }

  public void UpdateProfile(string name, string? avatar, string discriminator)
  {
    if (!string.IsNullOrEmpty(avatar) && !avatar.StartsWith("https://cdn.discordapp.com"))
    {
      throw new ArgumentException("Avatar should be absolute URL");
    }

    Name = name;
    Avatar = avatar;
    Discriminator = discriminator;
    LastRefreshedAt = SystemClock.Instance.GetCurrentInstant();
  }

  public void ToggleLockOut(bool shouldBeLockedOut)
  {
    if (shouldBeLockedOut == IsLockedOut)
    {
      return;
    }

    if (shouldBeLockedOut)
    {
      Unlock();
    }
    else
    {
      LockOut();
    }
  }

  public void SetEmail(string email, in bool isEmailConfirmed)
  {
    if (Email.Value == email && Email.IsConfirmed == isEmailConfirmed)
    {
      return;
    }

    Email = new Email(email, isEmailConfirmed);
  }

  public bool IsProfileInfoOutdated() =>
    LastRefreshedAt + ProfileValidityPeriod < SystemClock.Instance.GetCurrentInstant();

  private void LockOut()
  {
    if (IsLockedOut)
    {
      throw new CoreException("AlreadyLockedOut");
    }

    LockoutEnd = DateTimeOffset.MaxValue;
    AddDomainEvent(new IdentityLockedOut(Id, LockoutEnd.Value));
  }

  private void Unlock()
  {
    if (!IsLockedOut)
    {
      throw new CoreException("AlreadyUnlocked");
    }

    LockoutEnd = null;
    AddDomainEvent(new IdentityUnlocked(Id));
  }

  private void GenerateSecurityStamp()
  {
    SecurityStamp = Guid.NewGuid().ToString("N");
  }


  public DateTimeOffset? LockoutEnd
  {
    get => _lockoutEnd;
    set
    {
      _lockoutEnd = value;
      GenerateSecurityStamp();
    }
  }

  public void ReplaceDiscordRoles(IEnumerable<DiscordRoleInfo> roles)
  {
    _discordRoles.Clear();
    _discordRoles.AddRange(roles);
  }

  public string? Avatar { get; private set; }
  public Instant LastRefreshedAt { get; private set; }
  public ulong DiscordId { get; private set; }
  public string Discriminator { get; private set; } = null!;
  public string Name { get; set; } = null!;
  public string? StripeCustomerId { get; set; }

  public IReadOnlyList<DiscordRoleInfo> DiscordRoles => _discordRoles.AsReadOnly();

  /// <summary>Gets or sets the user name for this user.</summary>
  public virtual string UserName { get; set; } = null!;

  /// <summary>Gets or sets the normalized user name for this user.</summary>
  public virtual string NormalizedUserName { get; set; } = null!;

  /// <summary>Gets or sets the email address for this user.</summary>
  public Email Email { get; private set; } = null!;

  /// <summary>
  ///   Gets or sets a salted and hashed representation of the password for this user.
  /// </summary>
  public virtual string? PasswordHash { get; set; }

  /// <summary>
  ///   A random value that must change whenever a users credentials change (password changed, login removed)
  /// </summary>
  public virtual string SecurityStamp { get; set; } = null!;

  /// <summary>
  ///   Gets or sets a flag indicating if the user could be locked out.
  /// </summary>
  /// <value>True if the user could be locked out, otherwise false.</value>
  public virtual bool LockoutEnabled { get; set; }

  /// <summary>
  ///   Gets or sets the number of failed login attempts for the current user.
  /// </summary>
  public virtual int AccessFailedCount { get; set; }

  public bool IsLockedOut => LockoutEnd.GetValueOrDefault() > DateTimeOffset.UtcNow;
  public string? ConcurrencyStamp { get; private set; } = null!;
}