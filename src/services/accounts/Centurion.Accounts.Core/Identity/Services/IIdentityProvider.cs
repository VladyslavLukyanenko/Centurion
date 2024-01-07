namespace Centurion.Accounts.Core.Identity.Services;

public interface IIdentityProvider
{
  long? GetCurrentIdentity();
}