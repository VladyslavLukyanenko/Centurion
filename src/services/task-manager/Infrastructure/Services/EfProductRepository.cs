using Centurion.Contracts;
using Centurion.TaskManager.Core;
using Centurion.TaskManager.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Centurion.TaskManager.Infrastructure.Services;

public class EfProductRepository : IProductRepository
{
  private readonly DbContext _context;

  public EfProductRepository(DbContext context)
  {
    _context = context;
  }

  public async ValueTask<Product?> GetByIdAsync(Module module, string sku, CancellationToken ct = default)
  {
    return await _context.Set<Product>().FirstOrDefaultAsync(_ => _.Sku == sku && _.Module == module, ct);
  }

  public async ValueTask<Product> CreateAsync(Product entity, CancellationToken ct = default)
  {
    var e = await _context.AddAsync(entity, ct);
    return e.Entity;
  }

  public void Update(Product entity)
  {
    _context.Update(entity);
  }

  public async ValueTask<IList<Product>> GetByIdsAsync(IEnumerable<Product.CompositeId> productIds, CancellationToken ct)
  {
    var compositeIds = productIds.Select(_ => _.ToString());
    return await _context.Set<Product>()
      .Where(_ => compositeIds.Contains(_.Module + Product.CompositeId.Delim + _.Sku))
      .ToListAsync(ct);
  }
}