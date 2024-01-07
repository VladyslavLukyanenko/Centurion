using AutoMapper;
using Centurion.Contracts;
using Centurion.TaskManager.Application.Services;
using Centurion.TaskManager.Core;
using Microsoft.EntityFrameworkCore;

namespace Centurion.TaskManager.Infrastructure.Services;

public class EfProductProvider : IProductProvider
{
  private readonly DbContext _context;
  private readonly IMapper _mapper;

  public EfProductProvider(DbContext context, IMapper mapper)
  {
    _context = context;
    _mapper = mapper;
  }

  public async ValueTask<ProductData?> GetSharedByIdAsync(Product.CompositeId productId,
    CancellationToken ct = default)
  {
    var prod = await _context.Set<Product>()
      .AsNoTracking()
      .FirstOrDefaultAsync(_ => _.Module == productId.Module && _.Sku == productId.Sku, ct);

    return _mapper.Map<ProductData?>(prod);
  }

  public async ValueTask<IList<ProductData>> GetByIdsAsync(IEnumerable<Product.CompositeId> productIds,
    CancellationToken ct = default)
  {
    var compositeIds = productIds.Select(_ => _.ToString());
    var products = await _context.Set<Product>()
      .AsNoTracking()
      .Where(_ => compositeIds.Contains(_.Module + Product.CompositeId.Delim + _.Sku))
      .ToListAsync(ct);

    return products.Select(_mapper.Map<ProductData>).ToList();
  }

  public async ValueTask<IList<Product.CompositeId>> GetUnknownProducts(IEnumerable<Product.CompositeId> exceptIds,
    CancellationToken ct)
  {
    var products = _context.Set<Product>().AsNoTracking();
    var tasks = _context.Set<CheckoutTask>().AsNoTracking();
    var blacklist = exceptIds.Select(id => id.Module + Product.CompositeId.Delim + id.Sku);

    return await (
        from task in tasks
        join product in products on new { Sku = task.ProductSku, task.Module } equals new
          { product.Sku, product.Module } into tmp
        from product in tmp.DefaultIfEmpty()
        where product == null && !blacklist.Contains(task.Module + Product.CompositeId.Delim + task.ProductSku)
        select new Product.CompositeId
        {
          Module = task.Module,
          Sku = task.ProductSku
        }
      )
      .GroupBy(_ => _.Module + Product.CompositeId.Delim + _.Sku)
      .Select(_ => _.First())
      .ToListAsync(ct);
  }
}