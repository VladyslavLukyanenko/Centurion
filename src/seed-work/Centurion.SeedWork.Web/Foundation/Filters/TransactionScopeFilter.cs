using Centurion.SeedWork.Primitives;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Centurion.SeedWork.Web.Foundation.Filters;

public class TransactionScopeFilter : IAsyncActionFilter
{
  private static readonly Func<string, bool>[] SkipTxMethodNames =
  {
    HttpMethods.IsGet,
    HttpMethods.IsOptions,
    HttpMethods.IsHead,
    HttpMethods.IsTrace,
  };

  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<TransactionScopeFilter> _logger;

  public TransactionScopeFilter(IUnitOfWork unitOfWork, ILogger<TransactionScopeFilter> logger)
  {
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
  {
    if (SkipTxMethodNames.Any(isSkipMethod => isSkipMethod(context.HttpContext.Request.Method)))
    {
      await next.Invoke();
      return;
    }

    using var tx = await _unitOfWork.BeginTransactionAsync(false);
    ActionExecutedContext? executedContext = null;
    try
    {
      executedContext = await next.Invoke();
      if (executedContext.Exception == null)
      {
        await _unitOfWork.SaveEntitiesAsync();
        await tx.CommitAsync();
      }
    }
    catch (Exception exc)
    {
      _logger.LogError(exc, "Failed to commit transaction");
      await tx.RollbackAsync();
      if (executedContext != null)
      {
        executedContext.Result = new BadRequestResult();
      }
    }
  }
}