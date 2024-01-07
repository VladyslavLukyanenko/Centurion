using Centurion.Accounts.App.Identity.Model;
using NodaTime;

namespace Centurion.Accounts.App.Audit.Data;

public class ChangeSetData
{
  private IList<ChangeSetEntryRefData> _entries = new List<ChangeSetEntryRefData>();
  public Guid Id { get; set; }
  public string Label { get; set; } = null!;
  public Instant Timestamp { get; set; }

  public UserRef UpdatedBy { get; set; } = null!;

  public IList<ChangeSetEntryRefData> Entries
  {
    get => _entries;
    // ReSharper disable once ConstantConditionalAccessQualifier
    set => _entries = value?.OrderByDescending(_ => _.CreatedAt).ToList() ?? new List<ChangeSetEntryRefData>();
  }
}