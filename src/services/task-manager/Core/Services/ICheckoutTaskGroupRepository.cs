namespace Centurion.TaskManager.Core.Services;

public interface ICheckoutTaskGroupRepository
{
  ValueTask<CheckoutTaskGroup?> GetByIdAsync(Guid groupId, string userId, CancellationToken ct = default);
  void Remove(CheckoutTaskGroup group);
  ValueTask<CheckoutTaskGroup> CreateAsync(CheckoutTaskGroup group, CancellationToken ct = default);
  void Update(CheckoutTaskGroup group);
}