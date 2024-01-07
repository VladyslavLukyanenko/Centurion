namespace Centurion.TaskManager.Application.Services;

public interface ICheckoutTaskProvider
{
  ValueTask<IList<MappedTask>> GetMappedCheckoutTasksAsync(string userId, IEnumerable<Guid> taskIds,
    CancellationToken ct = default);

  ValueTask<MappedTask?> GetMappedCheckoutTaskAsync(string userId, Guid taskId,
    CancellationToken ct = default);

  ValueTask<int> GetTasksCountAsync(string userId, CancellationToken ct = default);
}