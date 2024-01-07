using Centurion.Contracts.CloudManager;
using DynamicData;

namespace Centurion.CloudManager.App.Services;

public interface ICloudManager
{
  public string ProviderName { get; }
  IObservable<IChangeSet<NodeInfo, string>> NodesInfo { get; }
  ValueTask KeepAlive(IEnumerable<KeyValuePair<string, KeepAliveCommand>> commands, CancellationToken ct = default);
}