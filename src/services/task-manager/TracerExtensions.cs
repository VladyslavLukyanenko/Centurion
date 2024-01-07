using Elastic.Apm.Api;

namespace Centurion.TaskManager;

public static class TracerExtensions
{
  public static ISpan? StartSpan(this ITracer tracer, string name, string type, string? subType = null,
    string? action = null)
  {
    var execCtx = tracer.GetExecutionSegment();
    return execCtx?.StartSpan(name, type, subType, action);
  }

  public static ITransaction? StartAttachedTransaction(this ITracer tracer, string name, string type)
  {
    var execCtx = tracer.GetExecutionSegment();
    return tracer.StartTransaction(name, type, execCtx?.OutgoingDistributedTracingData);
  }

  public static IExecutionSegment? GetExecutionSegment(this ITracer tracer)
  {
    ITransaction? transaction = tracer.CurrentTransaction;
    return tracer.CurrentSpan ?? (IExecutionSegment?)transaction;
  }
}