using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Centurion.Cli.Core.Config;
using Centurion.Cli.Core.Domain.LifecycleEvents;
using Centurion.Cli.Core.Services.LifecycleEvents;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace Centurion.Cli.Core.Services;

public class SecurityManager : ISecurityManager
{
  private readonly IIdentityService _identityService;
  private readonly ILogger<SecurityManager> _logger;
  private readonly SecurityConfig _securityConfig;
  private readonly ILifecycleEventManager _lifecycleEventManager;
  private readonly IGlobalRouter _globalRouter;

  public SecurityManager(IIdentityService identityService, SecurityConfig securityConfig,
    ILogger<SecurityManager> logger, ILifecycleEventManager lifecycleEventManager,
    IGlobalRouter globalRouter)
  {
    _identityService = identityService;
    _securityConfig = securityConfig;
    _logger = logger;
    _lifecycleEventManager = lifecycleEventManager;
    _globalRouter = globalRouter;
  }

  public void Spawn()
  {
    _logger.LogDebug("Spawning security manager");
    _identityService.IsAuthenticated
      .DistinctUntilChanged()
      .ObserveOn(RxApp.MainThreadScheduler)
      .Subscribe(a => { _ = HandleAuthentication(a); });

    _logger.LogDebug("Started authentication check by interval");
    // var isNotAuthenticated = _identityService.IsAuthenticated.Select(isAuthenticated => !isAuthenticated);
    Observable.Interval(_securityConfig.ReauthenticateInterval,
        RxApp.TaskpoolScheduler)
      .Do(_ => _identityService.TryAuthenticateAsync().AsTask().ToObservable())
      .Subscribe();
  }

  private async Task HandleAuthentication(bool isAuthenticated)
  {
    await _globalRouter.ShowTransitionView(isAuthenticated);
    _logger.LogDebug("Authentication state changed");
    if (isAuthenticated)
    {
      _logger.LogDebug("User is authenticated");
      await _lifecycleEventManager.DispatchAsync(UserAuthenticated.Instance);
      await _globalRouter.ShowMainView();
    }
    else
    {
      _logger.LogDebug("User isn't authenticated");
      await _lifecycleEventManager.DispatchAsync(UserLoggedOut.Instance);
      await _globalRouter.ShowLoginView();
    }
  }
}