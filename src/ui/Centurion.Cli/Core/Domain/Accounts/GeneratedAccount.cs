using Centurion.Cli.Core.Domain.Profiles;
using Centurion.Cli.Core.ViewModels;

namespace Centurion.Cli.Core.Domain.Accounts;

public class GeneratedAccount : ViewModelBase
{
  public GeneratedAccount(ProfileModel profile, Account account)
  {
    Profile = profile ?? throw new ArgumentNullException(nameof(profile));
    Account = account ?? throw new ArgumentNullException(nameof(account));
  }

  public ProfileModel Profile { get; }
  public Account Account { get; }
}