using CSharpFunctionalExtensions;
using Grpc.Core;
using Serilog;

namespace Centurion.Cli.Core.Services;

public static class Guard
{
  public static async ValueTask<Result> ExecuteSafe(Func<ValueTask> op)
  {
    try
    {
      await op();
      return Result.Success();
    }
    catch (RpcException exc)
    {
      return Result.Failure(exc.Status.Detail);
    }
    catch (Exception exc)
    {
      Log.Logger.Error(exc, "Failed to execute operation");
      return Result.Failure(exc.GetBaseException().Message);
    }
  }

  public static async ValueTask<Result<TResult>> ExecuteSafeFunc<TResult>(Func<ValueTask<TResult>> op)
  {
    try
    {
      return await op();
    }
    catch (RpcException exc)
    {
      return Result.Failure<TResult>(exc.Status.Detail);
    }
    catch (Exception exc)
    {
      Log.Logger.Error(exc, "Failed to execute operation");
      return Result.Failure<TResult>(exc.GetBaseException().Message);
    }
  }
}