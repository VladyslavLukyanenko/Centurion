using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MonitoringFakeShop.Pages;

public class ProductDetails : PageModel
{
  public Product Product { get; set; }
  public IActionResult OnGet(Guid id)
  {
    Product = InMemRepo.Products.FirstOrDefault(_ => _.Id == id);
    if (Product == null)
    {
      return NotFound();
    }

    return Page();
  }
}