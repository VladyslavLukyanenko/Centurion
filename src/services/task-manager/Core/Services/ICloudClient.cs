using Centurion.Contracts.CloudManager;

namespace Centurion.TaskManager.Core.Services;

public interface ICloudClient
{
  void KeepAlive(UserInfo userInfo);
  IObservable<NodeStatus> Connect(UserInfo userInfo);
}