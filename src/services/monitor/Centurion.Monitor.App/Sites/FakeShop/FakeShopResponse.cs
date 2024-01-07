namespace Centurion.Monitor.App.Sites.FakeShop;

public record FakeShopResponse(Guid Id, string? Name, bool IsAvailable)
{
  public FakeShopResponse()
    : this(Guid.Empty, null, false)
  {
  }
};