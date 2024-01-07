// using Centurion.Contracts;
// using Centurion.TaskManager.Core;
//
// namespace Centurion.TaskManager.Application.Services;
//
// public interface ITaskRegistry
// {
//   ValueTask<ISet<Guid>> GetInactiveTasksAsync(IEnumerable<Guid> taskIds, string userId,
//     CancellationToken ct = default);
//
//   ValueTask RegisterBatchAsync(IEnumerable<ActivatedTask> tasks, string userId,
//     CancellationToken ct = default);
//
//   ValueTask<IList<ActivatedTask>> GetActivatedTasksAsync(string userId, string site, string productId,
//     CancellationToken ct = default);
//
//   ValueTask<IDictionary<TaskStatusData, ISet<Guid>>> BatchUpdateStatusAsync(string userId, IEnumerable<Guid> taskIds,
//     TaskStatusData status);
//
//   ValueTask<IList<ActivatedTask>> GetActivatedTasksAsync(string userId, IEnumerable<Guid> taskIds,
//     CancellationToken ct = default);
//
//   ValueTask<bool> AlreadyStartedAsync(string userId, Guid taskId, CancellationToken ct = default);
//   // ValueTask<bool> AlreadyStartedAsync(string userId, IEnumerable<Guid> taskIds, CancellationToken ct = default);
//
//   ValueTask<ActivatedTask?> GetActivatedTaskAsync(string userId, Guid taskId,
//     CancellationToken ct = default);
// }