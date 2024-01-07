using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MonitoringFakeShop.Pages;

public class Shop : PageModel
{
  public int ItemsPerPage = 6;
  public List<Product> ProductsToDisplay { get; set; }
  public int PageIdx { get; set; }
  public int PagesCount { get; set; }
  public int TotalProductsCount { get; set; }
  public bool WithDelay { get; set; }

  public async Task<IActionResult> OnGetAsync(int pageIdx = 0, bool withDelay = false, CancellationToken ct = default)
  {
    var totalCount = InMemRepo.Products.Count(_ => _.IsAvailable);
    TotalProductsCount = totalCount;
    PageIdx = Math.Max(0, pageIdx);
    PagesCount = (int) Math.Ceiling(totalCount / (double) ItemsPerPage);
    ProductsToDisplay = InMemRepo.Products
      .Where(_ => _.IsAvailable)
      .OrderBy(_ => _.Index)
      .Skip(pageIdx * ItemsPerPage)
      .Take(ItemsPerPage)
      .ToList();

    WithDelay = withDelay;
    if (withDelay)
    {
      await Task.Delay(500, ct);
    }

    return Page();
  }
}