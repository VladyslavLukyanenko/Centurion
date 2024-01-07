using Centurion.TaskManager.Application;
using Centurion.TaskManager.Application.Services;
using Centurion.TaskManager.Core;
using Centurion.TaskManager.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Centurion.TaskManager.Infrastructure.Services;

public class EfCheckoutTaskProvider : ICheckoutTaskProvider
{
  private readonly DbContext _context;
  private readonly IProductRepository _productRepository;

  public EfCheckoutTaskProvider(DbContext context, IProductRepository productRepository)
  {
    _context = context;
    _productRepository = productRepository;
  }

  public async ValueTask<IList<MappedTask>> GetMappedCheckoutTasksAsync(string userId,
    IEnumerable<Guid> taskIds, CancellationToken ct)
  {
    var tasksSource = _context.Set<CheckoutTask>().AsNoTracking();
    var tasks = await tasksSource.Where(_ => _.UserId == userId && taskIds.Contains(_.Id))
      .ToArrayAsync(ct);

    var productIds = tasks.Select(_ => new Product.CompositeId(_.Module, _.ProductSku)).Distinct();
    var products = await _productRepository.GetByIdsAsync(productIds, ct);
    var productDict = products.ToDictionary(_ => _.GetCompositeId());

    return tasks.Select(t =>
    {
      if (!productDict.TryGetValue(new Product.CompositeId(t.Module, t.ProductSku), out var product))
      {
        const string defaultPicture = "https://accounts-api.centurion.gg/CenturionLogo__small.png";
        product = new Product(t.Module, t.ProductSku)
        {
          Name = t.ProductSku,
          Image = defaultPicture,
          Link = defaultPicture
        };
      }

      return new MappedTask(t, product);
    }).ToList();
  }

  public async ValueTask<MappedTask?> GetMappedCheckoutTaskAsync(string userId, Guid taskId,
    CancellationToken ct = default)
  {
    var list = await GetMappedCheckoutTasksAsync(userId, new[] {taskId}, ct);
    return list.FirstOrDefault();
  }

  public async ValueTask<int> GetTasksCountAsync(string userId, CancellationToken ct = default)
  {
    return await _context.Set<CheckoutTask>()
      .AsNoTracking()
      .CountAsync(_ => _.UserId == userId, ct);
  }
}