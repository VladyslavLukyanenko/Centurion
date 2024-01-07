using System.Collections.Concurrent;
using System.Reactive.Disposables;

namespace Centurion.Cli.Core.Services.Harvesters;

public class HarvesterRegistry : IHarvesterRegistry
{
  private static readonly ConcurrentDictionary<Guid, IHarvester> SpawnedHarvesters = new();
  private readonly HarvesterIterator _iterator = new(() => SpawnedHarvesters.Keys.ToArray());

  public IHarvester? Get(Guid harvesterId)
  {
    return SpawnedHarvesters.GetValueOrDefault(harvesterId);
  }

  public IHarvesterRegistryIterator GetIterator() => _iterator;

  public IDisposable Register(IHarvester harvester)
  {
    var harvesterId = harvester.Harvester.Id;
    SpawnedHarvesters[harvesterId] = harvester;
    _iterator.Invalidate();

    return Disposable.Create(() =>
    {
      SpawnedHarvesters.Remove(harvesterId, out _);
      _iterator.Invalidate();
    });
  }


  private class HarvesterIterator : IHarvesterRegistryIterator
  {
    private readonly SemaphoreSlim _gates = new(1, 1);

    private int _ix;
    private Guid[] _registeredIds;
    private Func<Guid[]> _idsProvider;


    public HarvesterIterator(Func<Guid[]> idsProvider)
    {
      _idsProvider = idsProvider;
      _registeredIds = _idsProvider();
    }

    public void Invalidate()
    {
      try
      {
        _gates.Wait(CancellationToken.None);
        _ix = 0;
        _registeredIds = _idsProvider();
      }
      finally
      {
        _gates.Release();
      }
    }

    public Guid? GetNextHarvesterId()
    {
      try
      {
        _gates.Wait(CancellationToken.None);
        if (_registeredIds.Length == 0)
        {
          return null;
        }

        if (_ix >= _registeredIds.Length)
        {
          _ix = 0;
        }

        return _registeredIds[_ix++];
      }
      finally
      {
        _gates.Release();
      }
    }
  }
}