using Centurion.Accounts.Core.Identity.Services;

namespace Centurion.Accounts.Infra.Audit.EntryValueConverters;

public class UserIdToRolesEntryValueConverter : Int64ToStringEntryValueConverterBase
{
  private readonly IUserRepository _userRepository;

  public UserIdToRolesEntryValueConverter(IUserRepository userRepository)
  {
    _userRepository = userRepository;
  }

  protected override async Task<string?> ConvertAsync(long id, CancellationToken ct = default)
  {
    var roleNames = await _userRepository.GetRolesAsync(id, ct);
    return string.Join(", ", roleNames);
  }
}