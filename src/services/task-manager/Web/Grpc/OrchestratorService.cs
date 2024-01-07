using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using AutoMapper;
using Centurion.Contracts;
using Centurion.Contracts.Checkout;
using Centurion.Contracts.TaskManager;
using Centurion.SeedWork.Web;
using Centurion.TaskManager.Application.Services;
using Centurion.TaskManager.Core.Services;
using Centurion.TaskManager.Web.Services;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using static Centurion.Contracts.TaskManager.Orchestrator;

namespace Centurion.TaskManager.Web.Grpc;

[Authorize]
public class OrchestratorService : OrchestratorBase
{
  private static readonly string HeaderNameClientId =
    Checkout.Descriptor.GetOptions().GetExtension(MessagesExtensions.CenturionHeaderUserId);

  private static readonly TimeSpan KeepAliveInterval = TimeSpan.FromSeconds(1);

  private readonly ITaskManager _taskManager;
  private readonly IMapper _mapper;
  private readonly ILogger<OrchestratorService> _logger;
  private readonly ICheckoutClientFactory _checkoutClientFactory;
  private readonly ICloudConnectionPool _connectionPool;
  private readonly ICloudClient _cloudClient;
  private readonly IUserInfoFactory _userInfoFactory;

  public OrchestratorService(ITaskManager taskManager, IMapper mapper, ILogger<OrchestratorService> logger,
    ICheckoutClientFactory checkoutClientFactory, ICloudConnectionPool connectionPool, ICloudClient cloudClient,
    IUserInfoFactory userInfoFactory)
  {
    _taskManager = taskManager;
    _mapper = mapper;
    _logger = logger;
    _checkoutClientFactory = checkoutClientFactory;
    _connectionPool = connectionPool;
    _cloudClient = cloudClient;
    _userInfoFactory = userInfoFactory;
  }

  public override async Task ConnectCloud(Empty request, IServerStreamWriter<CloudStatusResponse> responseStream,
    ServerCallContext context)
  {
    var ct = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
    var uinfo = _userInfoFactory.Create(context.GetHttpContext().User);
    await ExecuteUntilCancelled(async () =>
    {
      using var disposable = new CompositeDisposable();
      var destroy = new Subject<Unit>();

      disposable.Add(Disposable.Create(() =>
      {
        destroy.OnNext(Unit.Default);
        destroy.OnCompleted();
      }));

      _cloudClient.Connect(uinfo)
        .TakeUntil(destroy)
        .Select(s => new CloudStatusResponse
        {
          Status = s
        })
        .Catch<CloudStatusResponse, Exception>(exc =>
        {
          _logger.LogError(exc, "Error on writing statuses");
          return Observable.Empty<CloudStatusResponse>();
        })
        .Do(s => responseStream.WriteAsync(s).ToObservable())
        .Finally(() =>
        {
          ct.Cancel();
        })
        .Subscribe();

      while (!ct.IsCancellationRequested)
      {
        _cloudClient.KeepAlive(uinfo);

        await Task.Delay(KeepAliveInterval, ct.Token);
      }
    }, "Cloud", ct.Token);
  }

  public override async Task ConnectCheckout(IAsyncStreamReader<OrchestratorCommand> requestStream,
    IServerStreamWriter<CheckoutStatusChangedBatch> responseStream, ServerCallContext context)
  {
    var ct = context.CancellationToken;
    var userId = context.GetUserId();

    await ExecuteUntilCancelled(async () =>
    {
      var headers = new Metadata { { HeaderNameClientId, userId } };
      var opts = new CallOptions(headers, cancellationToken: ct);
      var checkoutClient = GetClientOrThrow(userId);
      using var cloudConn = _connectionPool.GetOrDefault(userId);
      using var conn = checkoutClient.ConnectCheckout(opts);

      _ = Task.Run(async () =>
      {
        await foreach (var clientCmd in requestStream.ReadAllAsync(ct))
        {
          if (clientCmd.CommandCase is OrchestratorCommand.CommandOneofCase.None)
          {
            continue;
          }

          var srvCmd = clientCmd.CommandCase switch
          {
            OrchestratorCommand.CommandOneofCase.Start => new CheckoutCommand
            {
              Start = await CreateStartCommand(clientCmd.Start, userId, ct)
            },
            OrchestratorCommand.CommandOneofCase.Stop => new CheckoutCommand
            {
              Stop = CreateStopCommand(clientCmd.Stop)
            },
            _ or OrchestratorCommand.CommandOneofCase.None => throw new ArgumentOutOfRangeException()
          };

          await conn.RequestStream.WriteAsync(srvCmd);
        }
      }, ct);

      await foreach (var status in conn.ResponseStream.ReadAllAsync(ct))
      {
        await responseStream.WriteAsync(status);
      }
    }, "Statuses/Commands", ct);
  }

  public override async Task ConnectRpc(IAsyncStreamReader<RpcMessage> requestStream,
    IServerStreamWriter<RpcMessage> responseStream, ServerCallContext context)
  {
    var ct = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
    var userId = context.GetUserId();
    await ExecuteUntilCancelled(async () =>
    {
      var checkoutClient = GetClientOrThrow(userId);
      var headers = new Metadata
      {
        { HeaderNameClientId, userId }
      };
      var opts = new CallOptions(headers, cancellationToken: ct.Token);
      using var cloudConn = _connectionPool.GetOrDefault(userId);
      using var conn = checkoutClient.ConnectRpc(opts);

      _ = Task.Run(async () =>
      {
        await foreach (var req in requestStream.ReadAllAsync(ct.Token))
        {
          await conn.RequestStream.WriteAsync(req);
        }
      }, ct.Token);

      await foreach (var rpcMessage in conn.ResponseStream.ReadAllAsync(ct.Token))
      {
        _logger.LogInformation("Sending RPC {Message} ({SessionID})", rpcMessage.PayloadCase, rpcMessage.SessionId);
        await responseStream.WriteAsync(rpcMessage);
      }
    }, "Rpc", ct.Token);
  }

  public override async Task<TasksExecutingStats> GetTasksStats(Empty request, ServerCallContext context)
  {
    var ct = context.CancellationToken;
    var checkoutClient = GetClientOrThrow(context.GetUserId());
    return await checkoutClient.GetTasksStatsAsync(request, cancellationToken: ct);
  }

  public override async Task<SupportedModuleList> GetSupportedModules(Empty request, ServerCallContext context)
  {
    var ct = context.CancellationToken;
    var checkoutClient = GetClientOrThrow(context.GetUserId());
    return await checkoutClient.GetSupportedModulesAsync(request, cancellationToken: ct);
  }

  private async ValueTask ExecuteUntilCancelled(Func<ValueTask> op, string logPrefix, CancellationToken ct)
  {
    while (!ct.IsCancellationRequested)
    {
      try
      {
        await op();
      }
      catch (OperationCanceledException)
      {
        if (ct.IsCancellationRequested)
        {
          break;
        }

        throw;
      }
      catch (RpcException exc)
      {
        if (exc.StatusCode == StatusCode.Cancelled)
        {
          _logger.LogDebug("{Prefix} connection aborted", logPrefix);
          break;
        }

        throw new RpcException(new Status(exc.StatusCode, logPrefix + ": " + exc.Status.Detail));
      }
      catch (IOException exc)
      {
        if (exc.InnerException is OperationCanceledException)
        {
          _logger.LogDebug("{Prefix} connection closed. Reason: {Exc}", logPrefix, exc.GetBaseException().Message);
        }
        else
        {
          _logger.LogWarning(exc, "{Prefix} connection terminated", logPrefix);
        }

        break;
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, "{Prefix} Error on processing", logPrefix);
      }
    }
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

  private static StopCheckoutCommand CreateStopCommand(StopTasksRequest stopRequest)
  {
    return new StopCheckoutCommand
    {
      Tasks =
      {
        stopRequest.TaskIds.Select(tid => new StopCheckoutDetails
        {
          Id = tid
        })
      }
    };
  }

  private async Task<StartCheckoutCommand> CreateStartCommand(StartTasksRequest request, string userId,
    CancellationToken ct)
  {
    var proxies = request.Proxies.ToDictionary(_ => Guid.Parse(_.Key), _ => _.Value);
    var profiles = request.Profiles
      .ToDictionary(_ => Guid.Parse(_.Key), _ => (ISet<ProfileData>)_.Value.Profiles.ToHashSet());

    var activations = request.TaskIds.Select(taskId => new TaskActivation(Guid.Parse(taskId), proxies, profiles));

    var tasks = await _taskManager.ActivateTasksAsync(userId, activations, ct);
    if (tasks.Count == 0)
    {
      throw new RpcException(new Status(StatusCode.FailedPrecondition, "Tasks already running"));
    }

    var commands = tasks.Select(t =>
    {
      var taskData = _mapper.Map<InitializedCheckoutTaskData>(t.Task);
      taskData.Product = _mapper.Map<ProductData>(t.Product);
      taskData.ProfileList.AddRange(t.ActivatedTask.Profiles);

      if (t.Task.CheckoutProxyPoolId.HasValue)
      {
        taskData.ProxyPool = t.ActivatedTask.CheckoutProxyPool;
      }

      taskData.Id = t.Task.Id.ToString();
      return taskData;
    });

    return new StartCheckoutCommand
    {
      Tasks = { commands }
    };
  }
}