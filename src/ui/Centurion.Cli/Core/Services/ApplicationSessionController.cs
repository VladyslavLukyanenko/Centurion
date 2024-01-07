using System.Collections.Concurrent;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using Centurion.Accounts.Announces.Hubs;
using Centurion.Cli.Core.Config;
using Centurion.Cli.Core.Services.Tasks;
using Centurion.Cli.Core.Services.ToastNotifications;
using Centurion.Contracts;
using Centurion.Contracts.Checkout.Integration;
using Centurion.Contracts.CloudManager;
using Centurion.Contracts.TaskManager;
using Centurion.TaskManager.Web.Hubs;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Humanizer;
using MessagePack;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using ReactiveUI;
using IRetryPolicy = Microsoft.AspNetCore.SignalR.Client.IRetryPolicy;

namespace Centurion.Cli.Core.Services;

// todo: refactor this god object!
public class ApplicationSessionController : IApplicationSessionController
{
  private static readonly SemaphoreSlim Gates = new(1, 1);
  private static readonly ConcurrentDictionary<Guid, TaskCompletionSource> PendingPings = new();
  private readonly CompositeDisposable _disposable = new();
  private readonly ILogger<ApplicationSessionController> _logger;
  private readonly IEnumerable<IAppStateHolder> _stateHolders;
  private readonly ITaskStatusRegistry _registry;
  private readonly IToastNotificationManager _toasts;
  private readonly Orchestrator.OrchestratorClient _orchestrator;
  private readonly IOrchestratorService _orchestratorService;

  private readonly HubConnectionHandle _tasksHub;
  private readonly HubConnectionHandle _announcesHub;

  private readonly IMessageBus _messageBus;


  private readonly AsyncPolicy _stateHoldersPolicy;

  private readonly IGeneralSettingsService _generalSettingsService;

  public ApplicationSessionController(ILogger<ApplicationSessionController> logger,
    ClientsConfig clientsConfig, IEnumerable<IAppStateHolder> stateHolders, ITaskStatusRegistry registry,
    ITokenProvider tokenProvider, ILoggerFactory loggerFactory, IToastNotificationManager toasts,
    IMessageBus messageBus, Orchestrator.OrchestratorClient orchestrator, IOrchestratorService orchestratorService,
    IGeneralSettingsService generalSettingsService)
  {
    const string tasksHubPath = "/hubs/task";
    const string announcesHubPath = "/hubs/announces";
    _logger = logger;
    var hubLogger = loggerFactory.CreateLogger<HubConnectionHandle>();
    var policy = Policy
      .Handle<InvalidOperationException>()
      .Or<Exception>()
      .Or<HubException>()
      .WaitAndRetryForeverAsync(attempt => attempt.ToJitteredDelayTime(), (_, retryAttempt, delay) =>
      {
        logger.LogError("Failed to connect to hub. Retry attempt {Attempt}. Next retry in {RetryDelay}",
          retryAttempt, delay);
      });

    _tasksHub = new HubConnectionHandle(hubLogger, policy, tokenProvider, clientsConfig.NotificationsUrl.ToString(),
      tasksHubPath, _ => _.AddMessagePackProtocol(o => o.SerializerOptions = MessagePackSerializer.DefaultOptions));

    _announcesHub = new HubConnectionHandle(hubLogger, policy, tokenProvider, clientsConfig.AccountsUrl.ToString(),
      announcesHubPath);

    _stateHolders = stateHolders;
    _registry = registry;
    _toasts = toasts;
    _messageBus = messageBus;
    _orchestrator = orchestrator;
    _orchestratorService = orchestratorService;
    _generalSettingsService = generalSettingsService;

    _stateHoldersPolicy = Policy
      .Handle<Exception>()
      .WaitAndRetryForeverAsync(attempt => attempt.ToJitteredDelayTime(), (_, retryAttempt, delay) =>
      {
        _toasts.Show(ToastContent.Warning($"Failure on app startup ({retryAttempt}). Retrying in {delay.Humanize()}"));
        logger.LogError(
          "Failed to connect initialize state holders. Retry attempt {Attempt}. Next retry in {RetryDelay}",
          retryAttempt, delay);
      });
  }

  public async ValueTask StartSession(CancellationToken ct = default)
  {
    await ConnectToBackend(ct);
  }

  private async Task ConnectToBackend(CancellationToken ct)
  {
    try
    {
      await Gates.WaitAsync(CancellationToken.None);
      _logger.LogDebug("User authenticated. Connecting to notifications hub");

      var cloudBackIsReady = new TaskCompletionSource();
      _ = Task.Run(async () =>
      {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _disposable.Add(Disposable.Create(cts, c =>
        {
          c.Cancel();
          c.Dispose();
        }));
        await ConnectToCheckoutBackend(cts.Token);
      }, ct);

      _ = Task.Run(async () =>
      {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _disposable.Add(Disposable.Create(cts, c =>
        {
          c.Cancel();
          c.Dispose();
        }));
        await ConnectToCloudBackend(cloudBackIsReady, cts.Token);
      }, ct);

      await cloudBackIsReady.Task;

      _tasksHub.Hub.On<ProductCheckedOut>(nameof(ITaskHubClient.ProductCheckOutSucceeded), @event =>
        {
          _logger.LogDebug("Product {SKU} checked out successfully", @event.Sku);
          _toasts.Show(ToastContent.Success($"Product {@event.Sku} checked out successfully",
            "Centurion AIO. Checked out!", ToastPriority.Important));

          _ = _generalSettingsService.PlayCheckoutSound(ct);
          _messageBus.SendMessage(@event);
        })
        .DisposeWith(_disposable);


      _tasksHub.Hub.On<Guid>(nameof(ITaskHubClient.Pong), state =>
        {
          if (PendingPings.TryRemove(state, out var tcs))
          {
            tcs.TrySetResult();
          }
        })
        .DisposeWith(_disposable);

      _announcesHub.Hub.On<string, string>(nameof(IAnnouncesHubClient.ReceiveAnnounce),
          (title, message) => { _toasts.Show(ToastContent.Information(message, title, ToastPriority.Important)); })
        .DisposeWith(_disposable);

      await Task.WhenAll(_tasksHub.Start(ct).AsTask(), _announcesHub.Start(ct).AsTask());
      SpawnHeartbeatWatcher(ct);
      await Task.Run(async () =>
      {
        foreach (var holder in _stateHolders)
        {
          holder.ResetCache();
        }

        var results = await Task.WhenAll(_stateHolders.Select(holder => _stateHoldersPolicy.ExecuteAsync(async _ =>
        {
          holder.ResetCache();
          return await holder.InitializeAsync(ct);
        }, ct, false)));

        var errorBuilder = new StringBuilder(results.Length);
        foreach (var failure in results.Where(_ => _.IsFailure))
        {
          errorBuilder.AppendLine(failure.Error);
        }

        if (errorBuilder.Length > 0)
        {
          var errorMessage = errorBuilder.ToString();
          _toasts.Show(ToastContent.Error(errorMessage));
          throw new InvalidOperationException(errorMessage);
        }
      }, ct);
    }
    finally
    {
      Gates.Release();
    }
  }

  private async Task ConnectToCheckoutBackend(CancellationToken ct)
  {
    CancellationTokenSource? tokenSource = null;
    do
    {
      tokenSource ??= CancellationTokenSource.CreateLinkedTokenSource(ct);
      try
      {
        var checkoutConn = _orchestrator.ConnectCheckout(cancellationToken: tokenSource.Token);
        var s = _orchestratorService.Commands
          .ObserveOn(RxApp.TaskpoolScheduler)
          .Catch<OrchestratorCommand, Exception>(exc =>
          {
            _logger.LogError(exc, "Error on commands sending");
            return Observable.Empty<OrchestratorCommand>();
          })
          .Do(cmd => checkoutConn.RequestStream.WriteAsync(cmd).ToObservable())
          .Subscribe();

        tokenSource.Token.Register(s.Dispose);

        await foreach (var changes in checkoutConn.ResponseStream.ReadAllAsync(tokenSource.Token))
        {
          foreach (var __ in changes.Changes.Values.Where(s => s.Category == TaskCategory.Declined))
          {
            _ = _generalSettingsService.PlayDeclineSound(ct);
          }

          _registry.UpdateStatus(
            changes.Changes.Select(change => KeyValuePair.Create(Guid.Parse(change.Key), change.Value)));
        }
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, "Error on connection to backend");
        tokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);

        if (!tokenSource.IsCancellationRequested)
        {
          await Task.Delay(TimeSpan.FromMilliseconds(500), tokenSource.Token);
        }
      }
    } while (!tokenSource.IsCancellationRequested);
  }

  private async Task ConnectToCloudBackend(TaskCompletionSource isReady, CancellationToken stoppingToken)
  {
    CancellationTokenSource? tokenSource = null;
    do
    {
      tokenSource ??= CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
      var ct = tokenSource.Token;
      try
      {
        var cloudConn = _orchestrator.ConnectCloud(new Empty(), cancellationToken: ct);

        await cloudConn.ResponseStream.ReadAllAsync(ct).FirstAsync(_ => _.Status is NodeStatus.Running, ct);

        isReady.TrySetResult();
        await foreach (var changes in cloudConn.ResponseStream.ReadAllAsync(ct))
        {
          _logger.LogDebug("Checkout service status {Status}", changes.Status);
        }
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, "Error on connection to cloud");
        tokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

        if (!tokenSource.IsCancellationRequested)
        {
          await Task.Delay(TimeSpan.FromMilliseconds(500), ct);
        }
      }
    } while (!tokenSource.IsCancellationRequested);

    isReady.TrySetCanceled(stoppingToken);
  }

  public async ValueTask StopSession(CancellationToken ct = default)
  {
    try
    {
      await Gates.WaitAsync(CancellationToken.None);
      _logger.LogDebug("User logged out. Disconnecting from notifications hub");
      _disposable.Clear();
      await _tasksHub.Stop(ct);
      await _announcesHub.Stop(ct);
      PendingPings.Clear();

      foreach (var stateHolder in _stateHolders)
      {
        stateHolder.ResetCache();
      }
    }
    finally
    {
      Gates.Release();
    }
  }

  private void SpawnHeartbeatWatcher(CancellationToken stoppingToken)
  {
    var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
    _disposable.Add(Disposable.Create(cts, c =>
    {
      c.Cancel();
      c.Dispose();
    }));
    var ct = cts.Token;
    _ = Task.Run(async () =>
    {
      while (!cts.IsCancellationRequested)
      {
        try
        {
          var state = Guid.NewGuid();
          var tcs = new TaskCompletionSource();
          PendingPings[state] = tcs;
          await _tasksHub.Hub.InvokeAsync("Ping", state.ToString(), ct);
          await tcs.WaitAsync(ct, (int)TimeSpan.FromSeconds(20).TotalMilliseconds);
          await Task.Delay(TimeSpan.FromSeconds(15), ct);
        }
        catch (OperationCanceledException)
        {
          await _tasksHub.EstablishConnectionAsync(ct);
          await _announcesHub.EstablishConnectionAsync(ct);
        }
        catch (Exception exc)
        {
          _logger.LogError(exc, "Failed to ping server");
          await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
      }
    }, ct);
  }

  private class ConnectionForeverRetryAndWaitPolicy : IRetryPolicy
  {
    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
      return retryContext.PreviousRetryCount.ToJitteredDelayTime();
    }
  }

  private class HubConnectionHandle
  {
    private readonly ILogger<HubConnectionHandle> _logger;
    private readonly AsyncRetryPolicy _policy;
    private readonly ITokenProvider _tokenProvider;
    private readonly string _hubPath;

    public HubConnectionHandle(ILogger<HubConnectionHandle> logger, AsyncRetryPolicy policy,
      ITokenProvider tokenProvider, string url, string hubPath,
      Action<IHubConnectionBuilder>? builderConfigurer = null)
    {
      _logger = logger;
      Hub = CreateTasksHubConnection(url, hubPath, builderConfigurer);
      _policy = policy;
      _tokenProvider = tokenProvider;
      _hubPath = hubPath;
    }

    public async ValueTask Start(CancellationToken ct = default)
    {
      Hub.Closed += HubTaskHubConnectionOnClosed;
      await EstablishConnectionAsync(ct);
    }

    public async ValueTask Stop(CancellationToken ct = default)
    {
      Hub.Closed -= HubTaskHubConnectionOnClosed;
      await Hub.StopAsync(ct);
    }

    public async Task EstablishConnectionAsync(CancellationToken ct = default)
    {
      _logger.LogInformation("Establishing connection with server {Path}", _hubPath);
      await _policy.ExecuteAsync(async () =>
      {
        if (ct.IsCancellationRequested)
        {
          return;
        }

        await Hub.StopAsync(ct);
        await Hub.StartAsync(ct);
      });
    }

    private HubConnection CreateTasksHubConnection(string url, string tasksHubPath,
      Action<IHubConnectionBuilder>? builderConfigurer = null)
    {
      var builder = new HubConnectionBuilder()
        .WithUrl(new UriBuilder(url) { Path = tasksHubPath }.Uri, o =>
        {
          // o.Transports = HttpTransportType.WebSockets;
          o.AccessTokenProvider = () => Task.FromResult(_tokenProvider.CurrentAccessToken);
        })
        .WithAutomaticReconnect(new ConnectionForeverRetryAndWaitPolicy());

      builderConfigurer?.Invoke(builder);

      var hubConnection = builder.Build();

      hubConnection.ServerTimeout = TimeSpan.FromSeconds(15);
      hubConnection.KeepAliveInterval = TimeSpan.FromSeconds(10);
      hubConnection.HandshakeTimeout = TimeSpan.FromSeconds(10);
      return hubConnection;
    }

    private async Task HubTaskHubConnectionOnClosed(Exception? exc)
    {
      _logger.LogError(0, exc, "Connection with {Path} closed because {Cause}", _hubPath,
        exc?.Message ?? "Graceful Stopping");
      if (exc is HubException or InvalidOperationException)
      {
        _logger.LogWarning("Restarting connection with {Path} because of unexpected disconnect with cause {Cause}",
          _hubPath, exc);
        await Task.Delay(1L.ToJitteredDelayTime());
        await EstablishConnectionAsync();
      }
    }

    public HubConnection Hub { get; }
  }
}