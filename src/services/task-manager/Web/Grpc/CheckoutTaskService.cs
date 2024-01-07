using System.Collections.Concurrent;
using AutoMapper;
using Centurion.Contracts;
using Centurion.Contracts.Checkout;
using Centurion.Contracts.Common;
using Centurion.Contracts.TaskManager;
using Centurion.SeedWork.Primitives;
using Centurion.SeedWork.Web;
using Centurion.TaskManager.Application.Services;
using Centurion.TaskManager.Core;
using Centurion.TaskManager.Core.Services;
using Centurion.TaskManager.Web.Services;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using CheckoutTask = Centurion.Contracts.TaskManager.CheckoutTask;

namespace Centurion.TaskManager.Web.Grpc;

[Authorize]
public class CheckoutTaskService : CheckoutTask.CheckoutTaskBase
{
  private const int MaxTaskCount = 1_000;
  private static readonly ConcurrentDictionary<string, SemaphoreSlim> Gates = new();

  private readonly ICheckoutTaskProvider _taskProvider;
  private readonly ICheckoutTaskRepository _taskRepository;
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ICheckoutClientFactory _checkoutClientFactory;
  private readonly ICloudConnectionPool _connectionPool;
  private readonly ICheckoutTaskGroupProvider _taskGroupProvider;
  private readonly ICheckoutTaskGroupRepository _taskGroupRepository;
  private readonly IProductProvider _productProvider;

  public CheckoutTaskService(ICheckoutTaskProvider taskProvider, ICheckoutTaskRepository taskRepository,
    IMapper mapper, IUnitOfWork unitOfWork, ICheckoutClientFactory checkoutClientFactory,
    ICloudConnectionPool connectionPool, ICheckoutTaskGroupProvider taskGroupProvider,
    ICheckoutTaskGroupRepository taskGroupRepository, IProductProvider productProvider)
  {
    _taskProvider = taskProvider;
    _taskRepository = taskRepository;
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _checkoutClientFactory = checkoutClientFactory;
    _connectionPool = connectionPool;
    _taskGroupProvider = taskGroupProvider;
    _taskGroupRepository = taskGroupRepository;
    _productProvider = productProvider;
  }

  public override async Task<CheckoutTaskGroupList> GetGroups(Empty request, ServerCallContext context)
  {
    var ct = context.CancellationToken;
    var groups = await _taskGroupProvider.GetListAsync(context.GetUserId(), ct);

    var productIds = groups.SelectMany(_ => _.Tasks).Select(t => new Product.CompositeId(t.Module, t.ProductSku))
      .Distinct();
    var products = await _productProvider.GetByIdsAsync(productIds, ct);
    return new CheckoutTaskGroupList
    {
      Groups = { groups },
      Products = { products }
    };
  }

  public override async Task<CheckoutTaskGroupData> SaveGroup(CheckoutTaskGroupData request, ServerCallContext context)
  {
    var ct = context.CancellationToken;
    var userId = context.GetUserId();
    CheckoutTaskGroup? group;
    if (IdsUtil.IsEmpty(request.Id))
    {
      group = new CheckoutTaskGroup
      {
        Name = request.Name,
        UserId = userId
      };

      await _taskGroupRepository.CreateAsync(group, ct);
    }
    else
    {
      var groupId = request.Id.ToGuidOrEmpty();
      group = await _taskGroupRepository.GetByIdAsync(groupId, userId, ct);
      if (group is null)
      {
        throw new RpcException(new Status(StatusCode.NotFound, "Task group not found"));
      }

      group.Name = request.Name;
      _taskGroupRepository.Update(group);
    }

    await _unitOfWork.SaveEntitiesAsync(ct);

    return _mapper.Map<CheckoutTaskGroupData>(group);
  }

  public override async Task<Empty> RemoveGroup(ByIdRequest request, ServerCallContext context)
  {
    var ct = context.CancellationToken;
    var groupId = request.Id.ToGuidOrEmpty();
    var userId = context.GetUserId();
    var group = await _taskGroupRepository.GetByIdAsync(groupId, userId, ct);
    if (group is null)
    {
      throw new RpcException(new Status(StatusCode.NotFound, "Task group not found"));
    }

    var tasks = await _taskRepository.GetByGroupIdAsync(groupId, userId, ct);
    if (tasks.Count != 0)
    {
      var checkoutClient = GetClientOrThrow(userId);
      await checkoutClient.ForceStopAsync(new ForceStopCheckoutCommand
      {
        Cmd = new StopCheckoutCommand
        {
          Tasks =
          {
            tasks.Select(t => new StopCheckoutDetails
            {
              Id = t.Id.ToString(),
            })
          }
        },
        UserId = userId
      });

      _taskRepository.Remove(tasks);
    }

    _taskGroupRepository.Remove(group);
    await _unitOfWork.SaveEntitiesAsync(ct);

    return new Empty();
  }

  public override async Task<Empty> BulkSaveTasks(CheckoutTaskList request, ServerCallContext context)
  {
    var ct = context.CancellationToken;
    var userId = context.GetUserId();

    var group = await _taskGroupRepository.GetByIdAsync(request.GroupId.ToGuidOrEmpty(), userId, ct);
    if (group is null)
    {
      throw new RpcException(new Status(StatusCode.NotFound, "Group does not exist"));
    }

    var newTasks = new List<CheckoutTaskData>(request.Tasks.Count);
    var toUpdateTasks = new List<CheckoutTaskData>(request.Tasks.Count);
    foreach (var data in request.Tasks)
    {
      var targetList = IdsUtil.IsEmpty(data.Id) ? newTasks : toUpdateTasks;
      targetList.Add(data);
    }

    if (newTasks.Any())
    {
      var lk = Gates.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
      try
      {
        await lk.WaitAsync(CancellationToken.None);

        var currCount = await _taskProvider.GetTasksCountAsync(userId, ct) + newTasks.Count;
        if (currCount > MaxTaskCount)
        {
          throw new RpcException(new Status(StatusCode.ResourceExhausted,
            $"Task limit reached. Available limit - {MaxTaskCount - currCount}"));
        }

        var toCreate = newTasks.Select(t => _mapper.Map(t, new Core.CheckoutTask
          {
            UserId = userId,
            GroupId = group.Id
          }))
          .ToArray();

        await _taskRepository.CreateAsync(toCreate, ct);
      }
      finally
      {
        lk.Release();
      }
    }

    if (toUpdateTasks.Any())
    {
      var taskIds = toUpdateTasks.Select(_ => _.Id.ToGuidOrEmpty());
      var tasks = await _taskRepository.GetByIdsAsync(taskIds, group.Id, ct);
      if (tasks.Count == 0)
      {
        throw new RpcException(new Status(StatusCode.NotFound, "Tasks not found"));
      }

      var taskLookup = tasks.ToDictionary(_ => _.Id);
      _taskRepository.Update(toUpdateTasks.Select(data => _mapper.Map(data, taskLookup[data.Id.ToGuidOrEmpty()])));
    }

    await _unitOfWork.SaveEntitiesAsync(ct);

    return new Empty();
  }

  public override async Task<Empty> BulkRemoveTasks(ByIdsRequest request, ServerCallContext context)
  {
    var ct = context.CancellationToken;
    var userId = context.GetUserId();
    var tasks = await _taskRepository.GetByIdsAsync(request.Ids.Select(id => id.ToGuidOrEmpty()), userId, ct);
    if (!tasks.Any())
    {
      throw new RpcException(new Status(StatusCode.NotFound, "Tasks not found"));
    }

    // if (await _taskRegistry.AlreadyStartedAsync(userId, task.Id, ct))
    // {
    //   throw new RpcException(new Status(StatusCode.FailedPrecondition, "Task is running"));
    // }

    var checkoutClient = GetClientOrThrow(userId);
    await checkoutClient.ForceStopAsync(new ForceStopCheckoutCommand
    {
      Cmd = new StopCheckoutCommand
      {
        Tasks =
        {
          request.Ids.Select(id => new StopCheckoutDetails
          {
            Id = id,
          })
        }
      },
      UserId = userId
    });

    _taskRepository.Remove(tasks);
    await _unitOfWork.SaveEntitiesAsync(ct);

    return new Empty();
  }

  private Checkout.CheckoutClient GetClientOrThrow(string userId)
  {
    var conn = _connectionPool.GetOrDefault(userId);
    if (conn is null)
    {
      throw new RpcException(new Status(StatusCode.Unavailable, "Cloud is not ready yet"));
    }

    return _checkoutClientFactory.Create(conn);
  }
}