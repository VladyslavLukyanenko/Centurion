using Centurion.Accounts.Core.Identity.Services;

namespace Centurion.Accounts.Infra.Audit.EntryValueConverters;

public class UserIdToFullNameEntryValueConverter : Int64ToStringEntryValueConverterBase
{
  private readonly IUserRepository _userRepository;

  public UserIdToFullNameEntryValueConverter(IUserRepository userRepository)
  {
    _userRepository = userRepository;
  }

  protected override async Task<string?> ConvertAsync(long id, CancellationToken ct = default)
  {
    var user = await _userRepository.GetByIdAsync(id, ct);
    return user?.UserName;
  }
}