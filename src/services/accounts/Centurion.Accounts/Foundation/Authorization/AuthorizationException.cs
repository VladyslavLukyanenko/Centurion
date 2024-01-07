using System.Runtime.Serialization;
using Centurion.Accounts.App;

namespace Centurion.Accounts.Foundation.Authorization;

public class AuthorizationException : AppException
{
  public AuthorizationException()
    : this("Operation is not permitted")
  {
  }

  public AuthorizationException(string? message)
    : base(message)
  {
  }

  public AuthorizationException(string? message, Exception? innerException)
    : base(message, innerException)
  {
  }

  protected AuthorizationException(SerializationInfo info, StreamingContext context)
    : base(info, context)
  {
  }
}