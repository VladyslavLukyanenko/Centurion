using Centurion.Cli.Core.Domain.Tasks;
using CSharpFunctionalExtensions;
using DynamicData;

namespace Centurion.Cli.Core.Services.Tasks;

public interface ITasksService : IAppStateHolder
{
  IObservableCache<CheckoutTaskGroupModel, Guid> TaskGroups { get; }
  ValueTask<Result> SaveGroup(CheckoutTaskGroupModel taskGroup, CancellationToken ct = default);
  ValueTask<Result> RemoveGroup(CheckoutTaskGroupModel taskGroup, CancellationToken ct = default);

  ValueTask<Result> BulkSave(Guid groupId, IEnumerable<CheckoutTaskModel> tasks, CancellationToken ct = default);
  ValueTask<Result> BulkRemove(Guid groupId, IEnumerable<CheckoutTaskModel> tasks, CancellationToken ct = default);
  ValueTask RefreshProductInfo(CancellationToken ct = default);
}