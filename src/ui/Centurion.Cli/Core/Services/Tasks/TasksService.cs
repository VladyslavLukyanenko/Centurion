using AutoMapper;
using Centurion.Cli.Core.Domain.Tasks;
using Centurion.Cli.Core.Services.Modules;
using Centurion.Contracts;
using Centurion.Contracts.Common;
using Centurion.Contracts.TaskManager;
using CSharpFunctionalExtensions;
using DynamicData;
using Google.Protobuf.WellKnownTypes;

namespace Centurion.Cli.Core.Services.Tasks;

public class TasksService : ExecutionStatusProviderBase, ITasksService
{
  private readonly SourceCache<CheckoutTaskGroupModel, Guid> _taskGroups = new(_ => _.Id);
  private readonly CheckoutTask.CheckoutTaskClient _checkoutTaskClient;
  private readonly IMapper _mapper;

  public TasksService(CheckoutTask.CheckoutTaskClient checkoutTaskClient, IMapper mapper)
  {
    _checkoutTaskClient = checkoutTaskClient;
    _mapper = mapper;
    TaskGroups = _taskGroups.AsObservableCache();
  }

  public IObservableCache<CheckoutTaskGroupModel, Guid> TaskGroups { get; }

  public ValueTask<Result> SaveGroup(CheckoutTaskGroupModel taskGroup, CancellationToken ct = default) =>
    Guard.ExecuteSafe(async () =>
      {
        var data = _mapper.Map<CheckoutTaskGroupData>(taskGroup);
        var r = await _checkoutTaskClient.SaveGroupAsync(data);
        var savedGroup = _mapper.Map<CheckoutTaskGroupModel>(r);
        _taskGroups.AddOrUpdate(savedGroup);
      })
      .TrackProgress(FetchingTracker);

  public ValueTask<Result> RemoveGroup(CheckoutTaskGroupModel taskGroup, CancellationToken ct = default) =>
    Guard.ExecuteSafe(async () =>
      {
        await _checkoutTaskClient.RemoveGroupAsync(new ByIdRequest
        {
          Id = taskGroup.Id.ToString()
        });

        _taskGroups.Remove(taskGroup.Id);
      })
      .TrackProgress(FetchingTracker);

  public ValueTask<Result> BulkSave(Guid groupId, IEnumerable<CheckoutTaskModel> tasks,
    CancellationToken ct = default) =>
    Guard.ExecuteSafe(async () =>
      {
        var toSave = tasks as CheckoutTaskModel[] ?? tasks.ToArray();
        var data = toSave.Select(_mapper.Map<CheckoutTaskData>);
        await _checkoutTaskClient.BulkSaveTasksAsync(new CheckoutTaskList
        {
          GroupId = groupId.ToString(),
          Tasks = { data }
        });
      })
      .TrackProgress(FetchingTracker);

  public ValueTask<Result> BulkRemove(Guid groupId, IEnumerable<CheckoutTaskModel> tasks,
    CancellationToken ct = default) =>
    Guard.ExecuteSafe(async () =>
      {
        var toRemove = tasks as CheckoutTaskModel[] ?? tasks.ToArray();
        await _checkoutTaskClient.BulkRemoveTasksAsync(new ByIdsRequest
        {
          Ids = { toRemove.Select(_ => _.Id.ToString()) }
        });

        var group = _taskGroups.Items.First(_ => _.Id == groupId);
        group.Tasks.Remove(toRemove);
      })
      .TrackProgress(FetchingTracker);

  public async ValueTask RefreshProductInfo(CancellationToken ct = default)
  {
    var groupList = await _checkoutTaskClient.GetGroupsAsync(new Empty(), cancellationToken: ct)
      .TrackProgress(FetchingTracker);
    foreach (var task in _taskGroups.Items.SelectMany(_ => _.Tasks.Items))
    {
      var product = groupList.Products.FirstOrDefault(_ => _.Module == task.Module && _.Sku == task.ProductSku);
      if (product is null)
      {
        continue;
      }

      task.ProductPicture = product.Image;
    }
  }

  public ValueTask<Result> InitializeAsync(CancellationToken ct = default) => Guard.ExecuteSafe(async () =>
  {
    var groupList = await _checkoutTaskClient.GetGroupsAsync(new Empty(), cancellationToken: ct)
      .TrackProgress(FetchingTracker);
    var groups = _mapper.Map<IList<CheckoutTaskGroupModel>>(groupList.Groups);
    foreach (var task in groups.SelectMany(_ => _.Tasks.Items))
    {
      var product = groupList.Products.FirstOrDefault(_ => _.Module == task.Module && _.Sku == task.ProductSku);
      if (product is null)
      {
        continue;
      }

      task.ProductPicture = product.Image;
    }

    _taskGroups.Edit(c => c.Load(groups));
  });

  public void ResetCache()
  {
    _taskGroups.Clear();
  }
}