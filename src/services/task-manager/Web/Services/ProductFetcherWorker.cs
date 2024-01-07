using AutoMapper;
using Centurion.Contracts;
using Centurion.SeedWork.Primitives;
using Centurion.TaskManager.Application.Services;
using Centurion.TaskManager.Core;
using Centurion.TaskManager.Core.Services;

namespace Centurion.TaskManager.Web.Services;

public class ProductFetcherWorker : BackgroundService
{
  private const int MaxAttempts = 5;

  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<ProductFetcherWorker> _logger;
  private readonly Dictionary<Product.CompositeId, int> _attemptsDict = new();

  public ProductFetcherWorker(IServiceProvider serviceProvider, ILogger<ProductFetcherWorker> logger)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        var sp = _serviceProvider.CreateScope().ServiceProvider;
        var productProvider = sp.GetRequiredService<IProductProvider>();
        var productRepository = sp.GetRequiredService<IProductRepository>();
        var clientFactory = sp.GetRequiredService<ICheckoutClientFactory>();
        var uow = sp.GetRequiredService<IUnitOfWork>();
        var mapper = sp.GetRequiredService<IMapper>();
        var pool = sp.GetRequiredService<ICloudConnectionPool>();
        var blacklist = sp.GetRequiredService<IBacklistedProductRepository>();

        var connections = pool.Connections.ToList();
        if (connections.Count == 0)
        {
          await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
          continue;
        }

        var unknownProducts = await productProvider.GetUnknownProducts(blacklist.BlacklistedProducts, stoppingToken);
        foreach (var unknownProduct in unknownProducts)
        {
          var conn = connections[Random.Shared.Next(connections.Count)];
          var client = clientFactory.Create(conn);
          try
          {
            var fetchedProduct = await client.FetchProductAsync(new FetchProductCommand
            {
              Module = unknownProduct.Module,
              Sku = unknownProduct.Sku
            }, cancellationToken: stoppingToken);

            var product = mapper.Map<Product>(fetchedProduct);
            await productRepository.CreateAsync(product, stoppingToken);
            await uow.SaveEntitiesAsync(stoppingToken);
          }
          catch (Exception exc)
          {
            _logger.LogError(exc, "Failed to fetch product info for {Sku} | {Module}", unknownProduct.Sku,
              unknownProduct.Module);

            var att = _attemptsDict.GetOrAdd(unknownProduct, static () => 0) + 1;
            _attemptsDict[unknownProduct] = att;
            if (att >= MaxAttempts)
            {
              await blacklist.Blacklist(unknownProduct, stoppingToken);
            }
          }
        }
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, "Failed to fetch product info");
      }

      await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
    }
  }
}