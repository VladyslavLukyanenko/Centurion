namespace Centurion.Monitor.Domain;

public delegate ValueTask AsyncEventHandler<in TEventArgs>(object? sender, TEventArgs args)
  where TEventArgs : AsyncEventArgs;