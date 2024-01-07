using Centurion.Accounts.App.Products.Model;

namespace Centurion.Accounts.App.Products.Services;

public interface IProductRefProvider
{
  ValueTask<ProductRef> GetRefAsync(long productId, CancellationToken ct = default);
}