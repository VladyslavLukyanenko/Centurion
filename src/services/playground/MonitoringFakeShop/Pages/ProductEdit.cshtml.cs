using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MonitoringFakeShop.Pages;

public class ProductEdit : PageModel
{
  public Product Product { get; set; }
  public string ReturnUrl { get; set; }

  public IActionResult OnGet(Guid id)
  {
    Product = InMemRepo.Products.FirstOrDefault(_ => _.Id == id);
    var referer = new Uri(Request.Headers["referer"]);
    ReturnUrl = referer.PathAndQuery;
    if (Product == null)
    {
      return NotFound();
    }

    return Page();
  }

  public IActionResult OnPost(Product product, string returnUrl)
  {
    InMemRepo.Update(product);
    return Redirect(returnUrl);
    // return RedirectToPage(new {id = product});
  }
}