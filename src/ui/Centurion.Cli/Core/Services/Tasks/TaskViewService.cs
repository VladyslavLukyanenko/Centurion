using AutoMapper;
using Centurion.Cli.Core.Domain.Tasks;
using Centurion.Cli.Core.Services.Modules;
using Centurion.Cli.Core.Services.Profiles;
using Centurion.Cli.Core.Services.Proxies;
using Centurion.Cli.Core.Validators;
using Centurion.Cli.Core.ViewModels.Tasks;
using Centurion.Contracts;
using Centurion.Contracts.TaskManager;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Centurion.Cli.Core.Services.Tasks;

public class TaskViewService : ExecutionStatusProviderBase,  ITaskViewService
{
  private readonly ITaskStatusRegistry _statusRegistry;
  private readonly IMapper _mapper;
  private readonly CheckoutTaskValidator _validator;
  private readonly ITasksService _tasksService;
  private readonly IProfilesRepository _profiles;
  private readonly IOrchestratorService _orchestratorService;
  private readonly Products.ProductsClient _productsClient;
  private readonly IProxyGroupsRepository _proxyGroupsRepository;
  private readonly ILogger<TaskViewService> _logger;

  public TaskViewService(ITaskStatusRegistry statusRegistry, IMapper mapper, CheckoutTaskValidator validator,
    ITasksService tasksService, IProfilesRepository profiles, IOrchestratorService orchestratorService,
    Products.ProductsClient productsClient, IProxyGroupsRepository proxyGroupsRepository,
    ILogger<TaskViewService> logger)
  {
    _statusRegistry = statusRegistry;
    _mapper = mapper;
    _validator = validator;
    _tasksService = tasksService;
    _profiles = profiles;
    _orchestratorService = orchestratorService;
    _productsClient = productsClient;
    _proxyGroupsRepository = proxyGroupsRepository;
    _logger = logger;
  }

  public async ValueTask<Result> Save(Guid groupId, IEnumerable<CheckoutTaskModel> tasks,
    CancellationToken ct = default)
  {
    var toSave = tasks as CheckoutTaskModel[] ?? tasks.ToArray();
    var validationResult = await ValidateTasks(toSave, ct).TrackProgress(FetchingTracker);
    if (validationResult.IsFailure)
    {
      return validationResult;
    }

    var (_, isFailure, error) = await _tasksService.BulkSave(groupId, toSave, ct);
    if (isFailure)
    {
      return Result.Failure(error);
    }

    _tasksService.ResetCache();
    await _tasksService.InitializeAsync(ct);
    _ = Task.Run(() => FetchProduct(toSave, ct), ct).TrackProgress(FetchingTracker);

    return Result.Success();
  }

  private async Task FetchProduct(CheckoutTaskModel[] toSave, CancellationToken cancellationToken)
  {
    var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    cts.CancelAfter(TimeSpan.FromMinutes(1));
    var ct = cts.Token;
    var proxyId = toSave.Select(_ => _.CheckoutProxyPoolId)
      .Where(_ => _.HasValue)
      .Select(_ => _!.Value)
      .FirstOrDefault();

    var skusToFetch = toSave.Select(t => (t.Module, t.ProductSku))
      .Distinct();


    try
    {
      IEnumerable<ProxyData> proxies = Array.Empty<ProxyData>();
      if (proxyId != default)
      {
        var proxy = _proxyGroupsRepository.Items.Lookup(proxyId).Value;
        var proxyPoolData = _mapper.Map<ProxyPoolData>(proxy);
        proxies = proxyPoolData.Proxies;
      }

      foreach (var (module, sku) in skusToFetch)
      {
        await _productsClient.FetchProductIfMissingAsync(new FetchProductCommand
        {
          Module = module,
          Proxies = { proxies },
          Sku = sku
        }, cancellationToken: ct);
      }

      // _tasksService.ResetCache();
      await _tasksService.RefreshProductInfo(ct);
    }
    catch (Exception exc)
    {
      _logger.LogError(exc, "Failed to fetch products");
    }
  }

  private async Task<Result> ValidateTasks(IEnumerable<CheckoutTaskModel> tasks, CancellationToken ct)
  {
    var validationResult = await Task.WhenAll(tasks.Select(task => _validator.ValidateAsync(task, ct)));
    if (validationResult.Any(_ => !_.IsValid))
    {
      var error = validationResult.First(_ => !_.IsValid).ToString();
      return Result.Failure(error);
    }

    return Result.Success();
  }

  public async ValueTask<Result> StartTask(IEnumerable<TaskRowViewModel> tasks, CancellationToken ct = default)
  {
    var taskToStart = tasks as TaskRowViewModel[] ?? tasks.ToArray();
    var validationResult = await ValidateTasks(taskToStart.Select(_ => _.Task), ct);
    if (validationResult.IsFailure)
    {
      return validationResult;
    }

    var availableProfiles = _profiles.LocalItems
      .SelectMany(_ => _.Profiles)
      .ToDictionary(_ => _.Id);

    var proxiesDict = new Dictionary<string, ProxyPoolData>();
    var taskIds = new List<string>(taskToStart.Length);
    var profilesDict = new Dictionary<string, ProfileList>();

    _statusRegistry.UpdateStatus(taskToStart.Select(t =>
      new KeyValuePair<Guid, TaskStatusData>(t.Task.Id, TaskStatusData.Starting)));

    foreach (var task in taskToStart)
    {
      var taskId = task.Task.Id.ToString();
      taskIds.Add(taskId);

      var profiles = task.Task.ProfileIds.Select(pid => availableProfiles[pid]);
      var profileData = _mapper.Map<IEnumerable<ProfileData>>(profiles);
      profilesDict[taskId] = new ProfileList
      {
        Profiles = { profileData }
      };

      if (task.CheckoutProxies is not null)
      {
        var checkoutProxyPool = _mapper.Map<ProxyPoolData>(task.CheckoutProxies);
        proxiesDict[taskId] = checkoutProxyPool;
      }

      if (task.MonitorProxies is not null)
      {
        var monitorProxyPool = _mapper.Map<ProxyPoolData>(task.MonitorProxies);
        proxiesDict[taskId] = monitorProxyPool;
      }
    }

    _orchestratorService.Send(new OrchestratorCommand
    {
      Start = new StartTasksRequest
      {
        TaskIds = { taskIds },
        Profiles = { profilesDict },
        Proxies = { proxiesDict },
      }
    });
    return Result.Success();
  }

  public ValueTask StopTask(IEnumerable<TaskRowViewModel> tasks, CancellationToken ct = default)
  {
    var toStop = tasks as TaskRowViewModel[] ?? tasks.ToArray();
    var taskIds = new List<string>(toStop.Length);
    var statusChanges = new List<KeyValuePair<Guid, TaskStatusData>>(toStop.Length);
    foreach (var task in toStop)
    {
      taskIds.Add(task.Task.Id.ToString());
      var status = task.Status == TaskStatusData.Terminating || task.Status == TaskStatusData.Terminated
        ? TaskStatusData.Terminated
        : TaskStatusData.Terminating;

      statusChanges.Add(KeyValuePair.Create(task.Task.Id, status));
    }

    _statusRegistry.UpdateStatus(statusChanges);

    _orchestratorService.Send(new OrchestratorCommand
    {
      Stop = new StopTasksRequest
      {
        TaskIds = { taskIds }
      }
    });

    return default;
  }

  public async ValueTask<Result> Create(Guid groupId, CheckoutTaskModel proto, int qty, CancellationToken ct = default)
  {
    var protoCopy = _mapper.Map<CheckoutTaskModel>(proto);
    return await Save(groupId, Enumerable.Range(0, qty).Select(_ => _mapper.Map<CheckoutTaskModel>(protoCopy)), ct);
  }

  public async ValueTask<Result> Duplicate(Guid groupId, IEnumerable<CheckoutTaskModel> tasks,
    CancellationToken ct = default)
  {
    var copy = _mapper.Map<CheckoutTaskModel[]>(tasks);
    return await Save(groupId, copy, ct);
  }
}