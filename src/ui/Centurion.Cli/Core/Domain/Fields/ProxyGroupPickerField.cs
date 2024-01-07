namespace Centurion.Cli.Core.Domain.Fields;

public class ProxyGroupPickerField : Field<ProxyGroup>
{
  private int _lastIdx;

  public ProxyGroupPickerField()
  {
  }

  public ProxyGroupPickerField(string displayName)
    : this(null, displayName, true)
  {
  }
    
  public ProxyGroupPickerField(string? systemName, string? displayName, bool isRequired)
    : base(systemName, displayName, isRequired)
  {
  }

  public Proxy GetNextAccount()
  {
    if (Value is null || !Value.HasAnyProxy)
    {
      throw new InvalidOperationException("Proxy group has no proxies");
    }

    if (_lastIdx >= Value.Proxies.Count)
    {
      _lastIdx = 0;
    }

    return Value.Proxies[_lastIdx++];
  }

  public override string? ValueId => Value?.Id.ToString();
  public override string? DisplayValue => Value?.Name;
}