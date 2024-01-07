namespace Centurion.SeedWork;

public static class PollyUtils
{
  private static readonly Random Random = new((int) DateTime.Now.Ticks);

  public static TimeSpan ToJitteredDelayTime(this long attempt, int maxAttemptDelayGrowCount = 5)
  {
    attempt = Math.Min(attempt, maxAttemptDelayGrowCount);
    var delay = (Random.NextDouble() + 2) * attempt;
    return TimeSpan.FromSeconds(delay);
  }

  public static TimeSpan ToJitteredDelayTime(this int attempt, int maxAttemptDelayGrowCount = 5) =>
    ((long) attempt).ToJitteredDelayTime(maxAttemptDelayGrowCount);
}
