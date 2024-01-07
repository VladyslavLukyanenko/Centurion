using Centurion.TaskManager.Core;

namespace Centurion.TaskManager.Web.Services;

public interface IBacklistedProductRepository
{
  ValueTask Blacklist(Product.CompositeId id, CancellationToken ct = default);
  IReadOnlyList<Product.CompositeId> BlacklistedProducts { get; }
}