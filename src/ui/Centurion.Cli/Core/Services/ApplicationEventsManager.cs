using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reflection;
using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Services.NotificationHandlers;
using MediatR;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace Centurion.Cli.Core.Services;

public class ApplicationEventsManager : IApplicationEventsManager
{
  private static readonly MethodInfo SubscribeHandlerMethod;

  static ApplicationEventsManager()
  {
    SubscribeHandlerMethod = ReflectionHelper
      .GetGenericMethodDef<ApplicationEventsManager>(_ => _.SubscribeHandler<IApplicationNotification>(null!));
  }

  private readonly IMessageBus _messageBus;
  private readonly ILogger<ApplicationEventsManager> _logger;
  private readonly IMediator _mediator;

  public ApplicationEventsManager(IMessageBus messageBus, ILogger<ApplicationEventsManager> logger,
    IMediator mediator)
  {
    _messageBus = messageBus;
    _logger = logger;
    _mediator = mediator;
  }

  public void Spawn()
  {
    _logger.LogDebug("Registering application event listeners");

    var notificationTypes = GetType().Assembly.GetExportedTypes()
      .Where(t => t.IsAssignableFrom(typeof(IApplicationNotification)));
    foreach (var h in notificationTypes)
    {
      var genericMethod = SubscribeHandlerMethod.MakeGenericMethod(h);
      genericMethod.Invoke(this, new object[] {h});
    }

    _logger.LogDebug("Listeners are registered");
  }

  private void SubscribeHandler<T>(ApplicationNotificationHandlerBase<T> handler)
    where T : IApplicationNotification
  {
    _messageBus.Listen<T>()
      .Do(m => _mediator.Publish(m).ToObservable())
      .Subscribe();
    _logger.LogDebug($"Registered {handler.GetType().Name}");
  }
}