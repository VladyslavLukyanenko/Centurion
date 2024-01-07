using Centurion.Contracts;
using Centurion.TaskManager.Core;

namespace Centurion.TaskManager.Application.Services;

public interface IProductProvider
{
  ValueTask<ProductData?> GetSharedByIdAsync(Product.CompositeId productId, CancellationToken ct = default);
  ValueTask<IList<ProductData>> GetByIdsAsync(IEnumerable<Product.CompositeId> productIds, CancellationToken ct = default);

  ValueTask<IList<Product.CompositeId>> GetUnknownProducts(IEnumerable<Product.CompositeId> exceptIds,
    CancellationToken ct);
}