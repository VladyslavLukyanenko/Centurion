using System.Diagnostics.CodeAnalysis;

namespace Centurion.SeedWork.Web.Foundation.Model;

public class ApiContract<T>
{
  public ApiContract([MaybeNull]T payload)
    : this(payload, null)
  {
  }

  public ApiContract(ApiError? error)
    : this(default!, error)
  {
  }

  protected ApiContract([MaybeNull] T payload, ApiError? error)
  {
    Payload = payload;
    Error = error;
  }

  public ApiError? Error { get; }

  [MaybeNull]
  public T Payload { get; }
}