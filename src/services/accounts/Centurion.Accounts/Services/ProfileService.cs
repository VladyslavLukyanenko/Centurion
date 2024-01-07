using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Identity.Services;

namespace Centurion.Accounts.Services;

public class ProfileService : IProfileService
{
  private readonly IUserRepository _userRepository;
  private readonly IUserClaimsPrincipalFactory<User> _userClaimsPrincipalFactory;

  public ProfileService(IUserRepository userRepository, IUserClaimsPrincipalFactory<User> userClaimsPrincipalFactory)
  {
    _userRepository = userRepository;
    _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
  }

  public async Task GetProfileDataAsync(ProfileDataRequestContext context)
  {
    string userSubject = context.Subject.GetSubjectId();
    var user = await _userRepository.GetByIdAsync(long.Parse(userSubject));
    if (user == null)
    {
      return;
    }

    var principal = await _userClaimsPrincipalFactory.CreateAsync(user);
    // context.AddRequestedClaims(principal.Claims);
    context.IssuedClaims.AddRange(principal.Claims);
    foreach (var claim in context.Subject.Claims)
    {
      if (!principal.HasClaim(claim.Type, claim.Value))
      {
        context.IssuedClaims.Add(claim);
      }
    }
  }

  public async Task IsActiveAsync(IsActiveContext context)
  {
    string userSubject = context.Subject.GetSubjectId();
    var user = await _userRepository.GetByIdAsync(long.Parse(userSubject));

#pragma warning disable CS8625
    context.IsActive = user != null
                       && !user.IsLockedOut
                       && user.Email.IsConfirmed;
#pragma warning restore
  }
}