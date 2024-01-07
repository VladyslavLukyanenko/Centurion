using Centurion.Accounts.App.Products.Model;

namespace Centurion.Accounts.App.Products.Services;

// ReSharper disable once InconsistentNaming
public static class IProductsRefProvidersExtensions
{
  public static async ValueTask InitializeAsync(this IProductRefProvider provider, ProductRef? @ref,
    CancellationToken ct = default)
  {
    if (@ref == null)
    {
      return;
    }

    var initialized = await provider.GetRefAsync(@ref.Id, ct);
    @ref.Name = initialized.Name;
  }

  public static async ValueTask InitializeAsync(this IPlanRefProvider provider, PlanRef? @ref,
    CancellationToken ct = default)
  {
    if (@ref == null)
    {
      return;
    }

    var initialized = await provider.GetRefAsync(@ref.Id, ct);
    @ref.Name = initialized.Name;
  }


  public static async ValueTask InitializeAsync(this IDashboardRefProvider provider, DashboardRef? @ref,
    CancellationToken ct = default)
  {
    if (@ref == null)
    {
      return;
    }

    var initialized = await provider.GetRefAsync(@ref.Id, ct);
    @ref.Name = initialized.Name;
  }

  public static async ValueTask InitializeAsync(this IDashboardRefProvider provider, IEnumerable<DashboardRef?> refs,
    CancellationToken ct = default)
  {
    refs = refs
      .Where(m => m != null)
      .Select(m => m!)
      .ToArray();

    var results = await provider.GetRefsAsync(refs.Select(_ => _!.Id), ct);
    foreach (var @ref in refs)
    {
      if (!results.TryGetValue(@ref!.Id, out var initialized))
      {
        continue;
      }

      @ref.Name = initialized.Name;
    }
  }
}