using NodaTime;

namespace Centurion.TaskManager.Core.Presets;

public class Preset : CheckoutTaskState
{
  public string ProductTitle { get; init; } = null!;
  public string ProductPicture { get; init; } = null!;
  public Instant ExpectedAt { get; init; }
}