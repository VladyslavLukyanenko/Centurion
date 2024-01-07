namespace Centurion.Monitor.Domain;

public static class AsyncEventHandlerExtensions
{
  public static async ValueTask InvokeIfNotEmptyAsync<T>(this AsyncEventHandler<T>? self, object sender, T args)
    where T : AsyncEventArgs
  {
    var pendingFiredHandlers = (self?.GetInvocationList() ?? Array.Empty<Delegate>())
      .Cast<AsyncEventHandler<T>>()
      .Select(async handler => await handler(sender, args));

    await Task.WhenAll(pendingFiredHandlers);
  }
}