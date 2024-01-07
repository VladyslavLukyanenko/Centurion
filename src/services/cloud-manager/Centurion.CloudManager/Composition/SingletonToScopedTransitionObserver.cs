using MassTransit;

namespace Centurion.CloudManager.Composition;

public class SingletonToScopedTransitionObserver<T> : IPublishObserver, ISendObserver
  where T : IPublishObserver, ISendObserver
{
  private readonly IServiceProvider _serviceProvider;

  public SingletonToScopedTransitionObserver(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
  }

  public Task PrePublish<T1>(PublishContext<T1> context) where T1 : class =>
    ExecuteOnImpl(_ => _.PrePublish(context));

  public Task PostPublish<T1>(PublishContext<T1> context) where T1 : class =>
    ExecuteOnImpl(_ => _.PostPublish(context));

  public Task PublishFault<T1>(PublishContext<T1> context, Exception exception) where T1 : class =>
    ExecuteOnImpl(_ => _.PublishFault(context, exception));

  public Task PreSend<T1>(SendContext<T1> context) where T1 : class => ExecuteOnImpl(_ => _.PreSend(context));

  public Task PostSend<T1>(SendContext<T1> context) where T1 : class => ExecuteOnImpl(_ => _.PostSend(context));

  public Task SendFault<T1>(SendContext<T1> context, Exception exception) where T1 : class =>
    ExecuteOnImpl(_ => SendFault(context, exception));

  private async Task ExecuteOnImpl(Func<T, Task> executor)
  {
    using var scope = _serviceProvider.CreateScope();
    var impl = scope.ServiceProvider.GetRequiredService<T>();

    await executor(impl);
  }
}