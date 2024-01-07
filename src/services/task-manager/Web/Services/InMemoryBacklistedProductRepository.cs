using System.Collections.Concurrent;
using Centurion.TaskManager.Core;

namespace Centurion.TaskManager.Web.Services;

public class InMemoryBacklistedProductRepository : IBacklistedProductRepository
{
  private static readonly object? Stub = default;
  private readonly ConcurrentDictionary<Product.CompositeId, object?> _blacklist = new();

  public ValueTask Blacklist(Product.CompositeId id, CancellationToken ct = default)
  {
    _blacklist.TryAdd(id, Stub);
    return default;
  }

  public IReadOnlyList<Product.CompositeId> BlacklistedProducts => _blacklist.Keys.ToList();
}