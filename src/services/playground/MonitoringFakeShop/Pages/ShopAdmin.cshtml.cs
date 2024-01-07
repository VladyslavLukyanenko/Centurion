using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MonitoringFakeShop.Pages;

public class ShopAdmin : PageModel
{
  public int ItemsPerPage = 20;
  public List<Product> ProductsToDisplay { get; set; }
  public int PageIdx { get; set; }
  public int PagesCount { get; set; }

  public void OnGet(int pageIdx = 0)
  {
    PageIdx = Math.Max(0, pageIdx);
    PagesCount = (int) Math.Ceiling(InMemRepo.Products.Count / (double) ItemsPerPage);
    ProductsToDisplay = InMemRepo.Products
      .OrderBy(_ => _.Index)
      .Skip(pageIdx * ItemsPerPage)
      .Take(ItemsPerPage)
      .ToList();
  }

  public IActionResult OnGetMove(Guid id, bool up)
  {
    var referer = new Uri(Request.Headers["referer"]);
    var retUrl = referer.PathAndQuery;
    var pIdx = InMemRepo.Products.FindIndex(_ => _.Id == id);
    if (pIdx == 0 && up)
    {
      return Redirect(retUrl);
    }


    if (pIdx == InMemRepo.Products.Count - 1 && !up)
    {
      return Redirect(retUrl);
    }

    var prev = InMemRepo.Products[up ? pIdx - 1 : pIdx + 1];
    var prevIdx = prev.Index;
    InMemRepo.Products[pIdx].Index = prevIdx;
    prev.Index = pIdx;
    InMemRepo.Update(prev);

    return Redirect(retUrl);
  }

  public IActionResult OnPostToggleActive(Guid id, bool active)
  {
    var referer = new Uri(Request.Headers["referer"]);
    var retUrl = referer.PathAndQuery;
    var p = InMemRepo.Products.FirstOrDefault(_ => _.Id == id);
    if (p == null)
    {
      return NotFound();
    }

    p.IsAvailable = active;


    return Redirect(retUrl);
  }
}