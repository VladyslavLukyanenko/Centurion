using CSharpFunctionalExtensions;
using NodaTime;

namespace Centurion.Accounts.Core.Products;

public class SuspensionLicensePeriod : Primitives.ValueObject
{
  private SuspensionLicensePeriod(Instant start, Instant? end = null)
  {
    Start = start;
    End = end;
  }

  protected override IEnumerable<object?> GetAtomicValues()
  {
    yield return Start;
    yield return End;
  }

  public static SuspensionLicensePeriod CreateStarted() =>
    new SuspensionLicensePeriod(SystemClock.Instance.GetCurrentInstant());

  public bool IsInProgress() => !End.HasValue || End > SystemClock.Instance.GetCurrentInstant();

  public Result Finish()
  {
    if (!IsInProgress())
    {
      return Result.Failure("Period already finished");
    }
      
    End = SystemClock.Instance.GetCurrentInstant();
    return Result.Success();
  }

  public Instant Start { get; private set; }
  public Instant? End { get; private set; }
}