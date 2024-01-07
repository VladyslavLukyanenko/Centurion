using System.Diagnostics.CodeAnalysis;

namespace Centurion.Accounts.Foundation.Model;

public class ApiContract<T>
{
  public ApiContract()
  {
  }

  public ApiContract([MaybeNull] T payload)
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

  public ApiError? Error { get; init; }
  [MaybeNull] public T Payload { get; init; } = default!;
}