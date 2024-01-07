using Microsoft.AspNetCore.Mvc;

namespace Centurion.SeedWork.Web.Foundation.ActionResults;

public class ValidationErrorResult
  : BadRequestObjectResult
{
  public ValidationErrorResult(Exception exc, string instance)
    : this(new[] {exc.Message}, instance)
  {
  }

  public ValidationErrorResult(IEnumerable<string> errors, string instance)
    : base(new ErrorDetails(errors))
  {
    Instance = instance;
  }

  public string Instance { get; }

  private class ErrorDetails
  {
    public ErrorDetails(IEnumerable<string> errors)
    {
      Errors = errors;
    }

    public IEnumerable<string> Errors { get; }
  }
}