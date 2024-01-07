using System.Collections.Concurrent;
using System.Reactive.Linq;
using Centurion.Cli.Core.Services.Harvesters;
using Centurion.Cli.Core.Services.ToastNotifications;
using Centurion.Contracts.Checkout;
using Centurion.Contracts.TaskManager;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace Centurion.Cli.Core.Services;

// todo: refactor
public class RpcManager : IRpcManager
{
  private readonly Orchestrator.OrchestratorClient _orchestrator;
  private readonly IHarvesterRegistry _harvesterRegistry;
  private readonly IIdentityService _identityService;
  private readonly ILogger<RpcManager> _logger;
  private readonly IToastNotificationManager _toasts;
  private readonly ISmsConfirmationManager _smsConfirmationManager;
  private readonly I3DS2Solver _3dsSolver;

  public RpcManager(Orchestrator.OrchestratorClient orchestrator, IHarvesterRegistry harvesterRegistry,
    IIdentityService identityService, ILogger<RpcManager> logger, IToastNotificationManager toasts,
    ISmsConfirmationManager smsConfirmationManager, I3DS2Solver solver)
  {
    _orchestrator = orchestrator;
    _harvesterRegistry = harvesterRegistry;
    _identityService = identityService;
    _logger = logger;
    _toasts = toasts;
    _smsConfirmationManager = smsConfirmationManager;
    _3dsSolver = solver;
  }

  public void Spawn()
  {
    CancellationTokenSource? cts = null;
    _identityService.IsAuthenticated
      .ObserveOn(RxApp.TaskpoolScheduler)
      .DistinctUntilChanged()
      .Subscribe(isAuthenticated =>
      {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        if (!isAuthenticated)
        {
          return;
        }

        StartRpcWorker(cts.Token);
      });
  }

  private void StartRpcWorker(CancellationToken ct)
  {
    _ = Task.Run(async () =>
    {
      var smsPrompts = new ConcurrentDictionary<string, Lazy<Task<string?>>>();
      while (!ct.IsCancellationRequested)
      {
        try
        {
          var conn = _orchestrator.ConnectRpc(cancellationToken: ct);
          var writer = new SynchronizedRpcMessageWriter(conn);
          var harvestersIterator = _harvesterRegistry.GetIterator();
          await foreach (var message in conn.ResponseStream.ReadAllAsync(ct))
          {
            _ = Task.Run(async () =>
            {
              try
              {
                // todo: enable sync by account
                // await gate.WaitAsync(CancellationToken.None);
                if (message.PayloadCase == RpcMessage.PayloadOneofCase.Init)
                {
                  var harvesterId = harvestersIterator.GetNextHarvesterId();
                  var harvester = _harvesterRegistry.Get(harvesterId.GetValueOrDefault());
                  if (harvester?.IsInitialized == false)
                  {
                    await writer.Write(new RpcMessage
                    {
                      ActionError = new HarvesterActionError(),
                      SessionId = message.SessionId
                    });
                    return;
                  }

                  await writer.Write(new RpcMessage
                  {
                    InitReply = new InitHarvesterReply
                    {
                      HarvesterId = harvesterId.ToString()
                    },
                    SessionId = message.SessionId
                  });
                }
                else if (message.PayloadCase == RpcMessage.PayloadOneofCase.SolveCaptcha)
                {
                  var harvesterId = Guid.Parse(message.SolveCaptcha.HarvesterId);
                  var harvester = _harvesterRegistry.Get(harvesterId);
                  if (harvester?.IsInitialized == false)
                  {
                    await writer.Write(new RpcMessage
                    {
                      ActionError = new HarvesterActionError
                      {
                        HarvesterId = harvesterId.ToString()
                      },
                      SessionId = message.SessionId
                    });
                    return;
                  }

                  var token = await harvester.TrySolveCaptcha(message.SolveCaptcha.ProductUrl);
                  await writer.Write(new RpcMessage
                  {
                    SessionId = message.SessionId,
                    SolveCaptchaReply = new SolveCaptchaHarvesterReply
                    {
                      Token = new ReCaptchaToken
                      {
                        CaptchaToken = token,
                        SolvedAt = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow)
                      }
                    }
                  });
                }
                else if (message.PayloadCase == RpcMessage.PayloadOneofCase.SmsConfirmation)
                {
                  var cmf = message.SmsConfirmation;
                  var key = $"{cmf.DisplayTaskId}_{cmf.PhoneNumber}";
                  var code = await smsPrompts.GetOrAdd(key, static (_, ctx) => new Lazy<Task<string?>>(() =>
                      ctx.Self._smsConfirmationManager.Prompt(ctx.Cmf.DisplayTaskId, ctx.Cmf.PhoneNumber).AsTask()),
                    (Self: this, Cmf: cmf)).Value;

                  var reply = new SmsConfirmationCommandReply
                  {
                    Skip = string.IsNullOrWhiteSpace(code)
                  };

                  if (!reply.Skip)
                  {
                    reply.SmsCode = code;
                  }

                  await writer.Write(new RpcMessage
                  {
                    SessionId = message.SessionId,
                    SmsConfirmationReply = reply
                  });
                }
                else if (message.PayloadCase == RpcMessage.PayloadOneofCase.SmsConfirmationAck)
                {
                  var cmf = message.SmsConfirmationAck;
                  var key = $"{cmf.DisplayTaskId}_{cmf.PhoneNumber}";
                  smsPrompts.TryRemove(key, out _);
                }
                else if (message.PayloadCase == RpcMessage.PayloadOneofCase.Solve3Ds2)
                {
                  var solveRequest = message.Solve3Ds2;
                  try
                  {
                    var solveResult = await _3dsSolver.Solve(solveRequest);
                    await writer.Write(new RpcMessage
                    {
                      SessionId = message.SessionId,
                      Solve3Ds2Reply = solveResult
                    });
                  }
                  catch (Solve3DS2Exception)
                  {
                    await writer.Write(new RpcMessage
                    {
                      SessionId = message.SessionId,
                      Solve3Ds2Reply = null,
                    });
                    throw;
                  }
                }
                else
                {
                  throw new ArgumentOutOfRangeException($"Message type '{message.PayloadCase}' is not supported.");
                }
              }
              catch (Exception exc)
              {
                _logger.LogError(exc, "Error on processing harvester command");
                _toasts.Show(ToastContent.Error(exc.GetBaseException().Message));
              }
              finally
              {
                // gate.Release();
              }
            }, ct);
          }
        }
        catch (Exception)
        {
          /*ignore*/
        }

        await Task.Delay(TimeSpan.FromMilliseconds(500), ct);
      }
    }, ct);
  }
}