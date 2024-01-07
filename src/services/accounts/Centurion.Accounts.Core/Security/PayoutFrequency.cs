using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.Security;

public class PayoutFrequency : Enumeration
{
  public static PayoutFrequency Daily = new(1, nameof(Daily), "0 0 12 * * ?");
  public static PayoutFrequency Weekly = new(2, nameof(Weekly), "0 0 12 ? * MON");
  public static PayoutFrequency Monthly = new(3, nameof(Monthly), "0 0 12 1 * ?");

  private PayoutFrequency(int id, string name, string cronPattern) : base(id, name)
  {
    CronPattern = cronPattern;
  }

  public string CronPattern { get; }
}