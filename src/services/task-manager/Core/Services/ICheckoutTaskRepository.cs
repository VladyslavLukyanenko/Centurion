namespace Centurion.TaskManager.Core.Services;

public interface ICheckoutTaskRepository
{
  ValueTask<CheckoutTask?> GetByIdAsync(Guid taskId, string userId, CancellationToken ct = default);

  ValueTask<IList<CheckoutTask>> GetByGroupIdAsync(Guid groupId, string userId, CancellationToken ct = default);

  ValueTask<IReadOnlyList<CheckoutTask>> GetByIdsAsync(IEnumerable<Guid> taskIds, string userId,
    CancellationToken ct = default);

  ValueTask<IReadOnlyList<CheckoutTask>> GetByIdsAsync(IEnumerable<Guid> taskIds, Guid groupId,
    CancellationToken ct = default);

  void Remove(CheckoutTask task);
  void Remove(IEnumerable<CheckoutTask> task);

  ValueTask<CheckoutTask> CreateAsync(CheckoutTask task, CancellationToken ct = default);
  ValueTask CreateAsync(IEnumerable<CheckoutTask> task, CancellationToken ct = default);

  void Update(CheckoutTask task);
  void Update(IEnumerable<CheckoutTask> task);
}