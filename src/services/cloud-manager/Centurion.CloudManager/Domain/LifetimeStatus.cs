namespace Centurion.CloudManager.Domain;

public enum LifetimeStatus
{
  PendingActivation,
  Active,
  ConnectionLost,
  PendingTermination,
  PendingShutDown
}