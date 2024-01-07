namespace Centurion.Cli.Core.Domain;

public enum CheckoutStatusKind
{
  Idle,
  Preparation,
  Ready,

  InProgress,
  Carted,
  Succeeded,
  Failed,
  Cancelled
}