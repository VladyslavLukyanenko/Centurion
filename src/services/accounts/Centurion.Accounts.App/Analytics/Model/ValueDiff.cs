namespace Centurion.Accounts.App.Analytics.Model;

public class ValueDiff<T>
{
  public static ValueDiff<int> CreateInt32(int curr, int prev) => new()
  {
    Current = curr,
    Previous = prev,
    ChangePercents = CalculatePercents(curr, prev)
  };

  public static ValueDiff<float> CreateSingle(float curr, float prev) => new()
  {
    Current = curr,
    Previous = prev,
    ChangePercents = CalculatePercents((decimal) curr, (decimal) prev)
  };

  public static ValueDiff<decimal> CreateDecimal(decimal curr, decimal prev) => new()
  {
    Current = decimal.Round(curr, 2),
    Previous = decimal.Round(prev, 2),
    ChangePercents = CalculatePercents(curr, prev)
  };

  private static float CalculatePercents(decimal curr, decimal prev) =>
    curr == 0
      ? curr == prev ? 0 : -100
      : (float) Math.Round((curr - prev) / curr * 100, 1);

  public T Current { get; set; } = default!;
  public T Previous { get; set; } = default!;

  public float ChangePercents { get; set; }
}