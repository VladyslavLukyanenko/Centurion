namespace Centurion.Accounts.Core.Services;

public interface IUserAgentService
{
  UserAgentDeviceType ResolveDeviceType(string userAgent);
}