using Centurion.SeedWork.Primitives;
using DynamicData.Binding;
using Newtonsoft.Json;

namespace Centurion.Cli.Core.Domain;

public class ProxyGroup : Entity
{
  private readonly List<Proxy> _availableProxies = new();
  private readonly List<Proxy> _busyProxies = new();
  private int _lastIdx;

  // public ProxyGroup()
  // {
  //   Proxies.ObserveCollectionChanges()
  //     .Subscribe(_ => Reset());
  // }

  [ /*BsonCtor, */JsonConstructor]
  public ProxyGroup(Guid id, string name)
    : base(id)
  {
    Name = name;
  }

  public ProxyGroup(string name, IEnumerable<Proxy> proxies)
    : this(Guid.Empty, name)
  {
    Proxies = new ObservableCollectionExtended<Proxy>(proxies);
  }

  public string Name { get; private set; }

  public ObservableCollectionExtended<Proxy> Proxies { get; private set; } = new();

  [JsonIgnore] public bool HasAnyProxy => Proxies.Any();

  public void RemoveProxy(Proxy proxy)
  {
    Proxies.Remove(proxy);
  }

  public Proxy GetNextProxy()
  {
    if (Proxies.Count == 0)
    {
      throw new InvalidOperationException("No proxies");
    }

    if (_lastIdx >= Proxies.Count)
    {
      _lastIdx = 0;
    }

    return Proxies[_lastIdx++];
  }


  private void Reset()
  {
    _availableProxies.Clear();
    _busyProxies.Clear();
    foreach (var proxy in Proxies)
    {
      RefreshProxy(proxy);
    }
  }

  private void RefreshProxy(Proxy proxy)
  {
    if (proxy.IsAvailable)
    {
      _availableProxies.Add(proxy);
      _busyProxies.Remove(proxy);
    }
    else
    {
      _busyProxies.Add(proxy);
      _availableProxies.Remove(proxy);
    }
  }

  public Proxy GetUnusedProxy()
  {
    var rnd = new Random((int) DateTime.Now.Ticks);
    if (_availableProxies.Count == 0)
    {
      var busyIdx = rnd.Next(0, _busyProxies.Count);
      return _busyProxies[busyIdx];
    }

    var idx = rnd.Next(0, _availableProxies.Count);
    var proxy = _availableProxies[idx];
    proxy.IsAvailable = false;

    RefreshProxy(proxy);
    return proxy;
  }

  public void TryReleaseProxy(Proxy? proxy)
  {
    if (proxy is null)
    {
      return;
    }

    proxy.IsAvailable = true;
    RefreshProxy(proxy);
  }
}