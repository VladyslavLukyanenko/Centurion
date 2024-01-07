using AutoMapper;
using Centurion.Contracts;
using Centurion.Contracts.Checkout;
using Centurion.Contracts.TaskManager;
using Centurion.SeedWork.Primitives;
using Centurion.SeedWork.Web;
using Centurion.TaskManager.Application.Services;
using Centurion.TaskManager.Core;
using Centurion.TaskManager.Core.Services;
using Centurion.TaskManager.Web.Services;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Centurion.TaskManager.Web.Grpc;

[Authorize]
public class ProductService : Products.ProductsBase
{
  private readonly IMapper _mapper;
  private readonly IProductRepository _productRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IProductProvider _productProvider;
  private readonly ICheckoutClientFactory _checkoutClientFactory;
  private readonly ICloudConnectionPool _connectionPool;

  public ProductService(IMapper mapper, IProductRepository productRepository, IUnitOfWork unitOfWork,
    IProductProvider productProvider, ICheckoutClientFactory checkoutClientFactory, ICloudConnectionPool connectionPool)
  {
    _mapper = mapper;
    _productRepository = productRepository;
    _unitOfWork = unitOfWork;
    _productProvider = productProvider;
    _checkoutClientFactory = checkoutClientFactory;
    _connectionPool = connectionPool;
  }

  public override async Task<ProductData?> GetBySku(BySkuRequest request, ServerCallContext context)
  {
    var ct = context.CancellationToken;
    return await _productProvider.GetSharedByIdAsync(new Product.CompositeId(request.Module, request.Sku), ct)
           ?? throw new RpcException(new Status(StatusCode.NotFound, "Product not found"));
  }

  public override async Task<ProductData> FetchProductIfMissing(FetchProductCommand request, ServerCallContext context)
  {
    var ct = context.CancellationToken;
    var userId = context.GetUserId();
    ProductData? existing =
      await _productProvider.GetSharedByIdAsync(new Product.CompositeId(request.Module, request.Sku), ct);
    if (existing == null)
    {
      var client = GetClientOrThrow(userId);
      existing = await client.FetchProductAsync(request, cancellationToken: ct);

      var fetched = _mapper.Map<Product>(existing);
      try
      {
        await _productRepository.CreateAsync(fetched, ct);
        await _unitOfWork.SaveEntitiesAsync(ct);
      }
      catch (DbUpdateException /*ignore*/)
      {
      }
    }

    return _mapper.Map<ProductData>(existing);
  }

  private Checkout.CheckoutClient GetClientOrThrow(string userId)
  {
    var conn = _connectionPool.GetOrDefault(userId);
    if (conn is null)
    {
      throw new RpcException(new Status(StatusCode.Unavailable, "Cloud is not ready yet"));
    }

    return _checkoutClientFactory.Create(conn);
  }
}