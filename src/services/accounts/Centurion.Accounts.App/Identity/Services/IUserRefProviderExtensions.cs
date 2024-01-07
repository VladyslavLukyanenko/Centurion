using Centurion.Accounts.App.Identity.Model;

namespace Centurion.Accounts.App.Identity.Services;

// ReSharper disable once InconsistentNaming
public static class IUserRefProviderExtensions
{
  public static async ValueTask InitializeAsync(this IUserRefProvider provider, UserRef? @ref,
    CancellationToken ct = default)
  {
    if (@ref == null)
    {
      return;
    }

    var initialized = await provider.GetRefAsync(@ref.Id, ct);
    @ref.Picture = initialized.Picture;
    @ref.FullName = initialized.FullName;
  }

  public static async ValueTask InitializeAsync(this IUserRefProvider provider, IEnumerable<UserRef?> refs,
    CancellationToken ct = default)
  {
    var userRefs = refs
      .Where(m => m != null)
      .Select(m => m!)
      .ToArray();

    var results = await provider.GetRefsAsync(userRefs.Select(_ => _.Id), ct);
    foreach (var userRef in userRefs)
    {
      if (!results.TryGetValue(userRef.Id, out var initialized))
      {
        continue;
      }

      userRef.Picture = initialized.Picture;
      userRef.FullName = initialized.FullName;
    }
  }
}