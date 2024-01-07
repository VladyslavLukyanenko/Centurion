using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MonitoringFakeShop.Pages;

[IgnoreAntiforgeryToken]
public class ProductsAdmin : PageModel
{
  [BindProperty] public bool IsPublished { get; set; }
    
  public IActionResult OnPost([FromForm] bool isPublished)
  {
    Product.DemoProduct.IsAvailable = isPublished;
    return RedirectToPage();
  }
    
  public void OnGet()
  {
    IsPublished = Product.DemoProduct.IsAvailable;
  }
}