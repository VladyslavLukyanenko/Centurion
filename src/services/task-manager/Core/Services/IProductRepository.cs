using Centurion.Contracts;

namespace Centurion.TaskManager.Core.Services;

public interface IProductRepository
{
  ValueTask<Product> CreateAsync(Product product, CancellationToken ct = default);
  ValueTask<Product?> GetByIdAsync(Module module, string sku, CancellationToken ct = default);
  void Update(Product product);
  ValueTask<IList<Product>> GetByIdsAsync(IEnumerable<Product.CompositeId> productIds, CancellationToken ct);
}