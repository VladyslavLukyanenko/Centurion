using Microsoft.AspNetCore.Mvc;

namespace MonitoringFakeShop.Controllers;

[ApiController]
[Route("fakeproduct")]
public class ProductsController : ControllerBase
{
  [HttpGet]
  public Product GetProduct() => Product.DemoProduct;
}