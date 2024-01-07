using System.Security.Claims;
using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.Identity;

public class UserClaim
  : Entity<long>
{
  private UserClaim()
  {
  }

  public UserClaim(long userId)
  {
    UserId = userId;
  }

  /// <summary>
  ///   Gets or sets the primary key of the user associated with this claim.
  /// </summary>
  public long UserId { get; }

  /// <summary>Gets or sets the claim type for this claim.</summary>
  public string ClaimType { get; private set; } = null!;

  /// <summary>Gets or sets the claim value for this claim.</summary>
  public string ClaimValue { get; private set; } = null!;

  /// <summary>Converts the entity into a Claim instance.</summary>
  /// <returns></returns>
  public virtual Claim ToClaim()
  {
    return new Claim(ClaimType, ClaimValue);
  }

  /// <summary>Reads the type and value from the Claim.</summary>
  /// <param name="claim"></param>
  public virtual void InitializeFromClaim(Claim claim)
  {
    ClaimType = claim.Type;
    ClaimValue = claim.Value;
  }
}