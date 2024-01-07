using Centurion.Contracts;

namespace Centurion.TaskManager.Application.Services;

public interface ICheckoutTaskGroupProvider
{
  ValueTask<IList<CheckoutTaskGroupData>> GetListAsync(string userId, CancellationToken ct = default);
}