using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Centurion.CloudManager.App.Services;
using Centurion.CloudManager.Domain;
using Centurion.Contracts.CloudManager;
using DynamicData;
using Grpc.Core;

namespace Centurion.CloudManager.Web.Grpc;

public class CloudService : Cloud.CloudBase
{
  private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(5);

  private readonly ICloudManager _cloudManager;

  public CloudService(IEnumerable<ICloudManager> cloudManagers)
  {
    _cloudManager = cloudManagers.First(_ => _.ProviderName == Clouds.AWS);
  }

  public override async Task Connect(IAsyncStreamReader<CloudCommandBatch> requestStream,
    IServerStreamWriter<NodeInfoBatch> responseStream, ServerCallContext context)
  {
    var disposable = new CompositeDisposable();
    var ct = context.CancellationToken;
    ct.Register(disposable.Dispose);

    var destroy = new Subject<Unit>();
    disposable.Add(Disposable.Create(destroy, d =>
    {
      d.OnNext(Unit.Default);
      d.OnCompleted();
    }));

    _cloudManager.NodesInfo
      .Filter(_ => !string.IsNullOrEmpty(_.UserId))
      .TakeUntil(destroy)
      .ToCollection()
      .Select(_ => _.GroupBy(q => q.UserId).ToDictionary(q => q.Key, q => q.First()))
      .Subscribe(info => responseStream.WriteAsync(new NodeInfoBatch
      {
        PerUserInfo = { info }
      }));

    await foreach (var batch in requestStream.ReadAllAsync(ct))
    {
      await _cloudManager.KeepAlive(batch.PerUserCommands.Where(IsNotTimedOut), ct);
    }
  }

  private bool IsNotTimedOut(KeyValuePair<string, KeepAliveCommand> arg)
  {
    return arg.Value.Timestamp.ToDateTimeOffset() + CommandTimeout > DateTimeOffset.UtcNow;
  }
}