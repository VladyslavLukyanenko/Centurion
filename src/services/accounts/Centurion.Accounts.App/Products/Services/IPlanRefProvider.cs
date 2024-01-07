using Centurion.Accounts.App.Products.Model;

namespace Centurion.Accounts.App.Products.Services;

public interface IPlanRefProvider
{
  ValueTask<PlanRef> GetRefAsync(long planId, CancellationToken ct = default);
}