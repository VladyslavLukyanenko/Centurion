using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.Security;

public class Currency : Enumeration
{
  public static readonly Currency Usd = new(1, "USD");

  private Currency(int id, string name) : base(id, name)
  {
  }
}