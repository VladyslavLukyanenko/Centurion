// using Centurion.Contracts;
// using Centurion.TaskManager.Application.Services;
// using Centurion.TaskManager.Core;
// using StackExchange.Redis;
// using StackExchange.Redis.Extensions.Core.Abstractions;
//
// namespace Centurion.TaskManager.Infrastructure.Services;
//
// public class RedisBasedTaskRegistry : ITaskRegistry
// {
//   private static readonly TimeSpan StatusValidityLifetime = TimeSpan.FromSeconds(30);
//   private static readonly TimeSpan StatusInProgressLifetime = StatusValidityLifetime;
//   private readonly IRedisDatabase _redis;
//
//   public RedisBasedTaskRegistry(IRedisDatabase redis)
//   {
//     _redis = redis;
//   }
//
//   public async ValueTask<ISet<Guid>> GetInactiveTasksAsync(IEnumerable<Guid> taskIds, string userId,
//     CancellationToken ct = default)
//   {
//     var allIds = new HashSet<Guid>(taskIds);
//     var keys = allIds.Select(taskId => CreateTaskRedisKey(userId, taskId)).ToArray();
//     var entries = await _redis.GetAllAsync<ActivatedTask>(keys);
//
//     var activeIds = entries.Where(it => it.Value != null)
//       .Where(_ => !_.Value.Status.IsNotRunning())
//       .Select(_ => _.Value.TaskId);
//
//     allIds.ExceptWith(activeIds);
//
//     return allIds;
//   }
//
//   public async ValueTask RegisterBatchAsync(IEnumerable<ActivatedTask> tasks, string userId,
//     CancellationToken ct = default)
//   {
//     var taskList = tasks as IList<ActivatedTask> ?? tasks.ToArray();
//     var toAdd = taskList
//       .Select(task => new Tuple<string, ActivatedTask>(CreateTaskRedisKey(userId, task.TaskId), task))
//       .ToArray();
//
//     await _redis.AddAllAsync(toAdd, StatusValidityLifetime);
//   }
//
//   public async ValueTask<IList<ActivatedTask>> GetActivatedTasksAsync(string userId, string site, string productId,
//     CancellationToken ct = default)
//   {
//     var keys = await _redis.SearchKeysAsync($"tasks.{userId}.*");
//     var entries = await _redis.GetAllAsync<ActivatedTask>(keys);
//     return entries.Select(_ => _.Value)
//       .Where(it => it != null && it.Site == site && it.Sku == productId)
//       .ToList();
//   }
//
//   public async ValueTask<IDictionary<TaskStatusData, ISet<Guid>>> BatchUpdateStatusAsync(string userId,
//     IEnumerable<Guid> taskIds, TaskStatusData status)
//   {
//     var keys = taskIds.Select(id => CreateTaskRedisKey(userId, id));
//     var entries = (await _redis.GetAllAsync<ActivatedTask>(keys)).Where(it => it.Value != null).ToArray();
//
//     var tx = _redis.Database.CreateTransaction();
//     _ = tx.KeyDeleteAsync(entries.Select(_ => (RedisKey)_.Key).ToArray());
//
//     var statuses = new Dictionary<TaskStatusData, ISet<Guid>>(entries.Length);
//     foreach (var (key, activatedTask) in entries)
//     {
//       var lifetime = !activatedTask.UpdateStatusIfInProgress(status)
//         ? StatusValidityLifetime
//         : StatusInProgressLifetime;
//       var value = _redis.Serializer.Serialize(activatedTask);
//       var expiry = DateTimeOffset.UtcNow - activatedTask.UpdatedAt + lifetime;
//       _ = tx.StringSetAsync(key, value, expiry);
//
//       var list = statuses.GetOrAdd(activatedTask.Status, entries.Length, capacity => new HashSet<Guid>(capacity));
//       list.Add(activatedTask.TaskId);
//     }
//
//     await tx.ExecuteAsync();
//
//     return statuses;
//   }
//
//   public async ValueTask<IList<ActivatedTask>> GetActivatedTasksAsync(string userId, IEnumerable<Guid> taskIds,
//     CancellationToken ct = default)
//   {
//     var keys = taskIds.Select(id => CreateTaskRedisKey(userId, id));
//     var e = await _redis.GetAllAsync<ActivatedTask>(keys);
//     return e.Values.Where(it => it != null).ToList();
//   }
//
//   public async ValueTask<ActivatedTask?> GetActivatedTaskAsync(string userId, Guid taskId,
//     CancellationToken ct = default)
//   {
//     var keys = CreateTaskRedisKey(userId, taskId);
//     return await _redis.GetAsync<ActivatedTask>(keys);
//   }
//
//   public async ValueTask<bool> AlreadyStartedAsync(string userId, Guid taskId, CancellationToken ct = default)
//   {
//     var e = await GetActivatedTaskAsync(userId, taskId, ct);
//     return IsRunning(e);
//   }
//
//   private static bool IsRunning(ActivatedTask? task) => task?.Status.IsNotRunning() == false;
//   private static string CreateTaskRedisKey(string userId, Guid taskId) => $"tasks.{userId}.{taskId}";
// }