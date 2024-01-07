namespace Centurion.Accounts.Foundation.Model;

public class ApiError
{
  public ApiError()
  {
  }

  public ApiError(Exception exc)
    : this(exc.Message)
  {
  }

  public ApiError(string code, object? details = null)
  {
    Message = code;
    Details = details;
  }

  public string Message { get; init; } = null!;
  public object? Details { get; init; } = null!;
}