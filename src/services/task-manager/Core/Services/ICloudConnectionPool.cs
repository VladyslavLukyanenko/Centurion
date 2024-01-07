namespace Centurion.TaskManager.Core.Services;

public interface ICloudConnectionPool
{
  ICloudConnection? GetOrDefault(string userId);
  IEnumerable<ICloudConnection> Connections { get; }
}