namespace Centurion.Accounts.Core.Audit.Services;

public interface IChangeSetEntryValueConverter
{
  Task<string?> ConvertAsync(string? value, CancellationToken ct = default);
}