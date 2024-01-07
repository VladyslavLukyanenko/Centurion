using Elastic.Apm;
using LiteDB;
using Serilog;
using Splat;

namespace Centurion.Cli;

public static class LifetimeUtil
{
  public static async ValueTask GracefullyTerminateProcesses()
  {
    try
    {
      var db = Locator.Current.GetService<ILiteDatabase>();
      db?.Dispose();
    }
    catch (OperationCanceledException /* expected */)
    {
    }

    Log.CloseAndFlush();
    var apmAgent = Locator.Current.GetService<IApmAgent>();
    if (apmAgent != null)
    {
      await apmAgent.Flush().ConfigureAwait(false);
    }
  }
}