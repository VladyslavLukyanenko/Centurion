using System.Reactive.Subjects;
using CSharpFunctionalExtensions;
using NodaTime;

namespace Centurion.CloudManager.Domain;

public abstract class Node : IDisposable
{
  private const string ExpectedDockerApiPort = ":17523";
  // private static readonly Duration FreeNodesReadyDelay = Duration.FromSeconds(10);
  private readonly BehaviorSubject<NodeStatus> _status = new(NodeStatus.Unknown);
  private readonly BehaviorSubject<NodeStage> _stage = new(new NodeStage(LifetimeStatus.PendingTermination));
  private string? _publicDnsName;
  private readonly List<RuntimeImageInfo> _images = new();

  protected Node(string id)
  {
    Id = id;
  }

  internal void Bind(NodeSnapshot snapshot, LifetimeStatus status)
  {
    if (User == snapshot.User)
    {
      return;
    }

    if (User is not null)
    {
      throw new InvalidOperationException($"Can't bind node {Id} because its already bound to {User.Id}");
    }

    User = snapshot.User;
    ChangeStage(status);
    snapshot.AttachTo(this);
  }

  internal void Reuse(UserInfo user)
  {
    if (User is not null)
    {
      throw new InvalidOperationException($"Can't bind node {Id} because its already bound to {User.Id}");
    }

    User = user;
    ChangeStage(LifetimeStatus.PendingActivation);
  }

  internal void Unbind()
  {
    User = null;
  }

  public void UpdateStatus(NodeStatus s) => _status.OnNext(s);

  public void SetImages(IEnumerable<RuntimeImageInfo> infos)
  {
    _images.Clear();
    _images.AddRange(infos);
  }

  public void Checked(bool isAvailable)
  {
    LastChecked = SystemClock.Instance.GetCurrentInstant();
    IsReady = Status is NodeStatus.Running && isAvailable;
  }

  public void Dispose()
  {
    _status.Dispose();
  }

  public Result ChangeStage(LifetimeStatus status)
  {
    if (status is LifetimeStatus.PendingShutDown && Stage.Status is not LifetimeStatus.PendingTermination)
    {
      return Result.Failure("Termination allowed only from pending termination state");
    }

    if (status is LifetimeStatus.PendingTermination && Stage.Status is not LifetimeStatus.ConnectionLost)
    {
      return Result.Failure("PendingTermination allowed only from conn lost state");
    }

    _stage.OnNext(new NodeStage(status));
    return Result.Success();
  }

  public void ClearImages()
  {
    _images.Clear();
  }

  public bool IsConnectionLost(IEnumerable<string> userIds) =>
    User is not null
    && Stage.Status is LifetimeStatus.ConnectionLost
    && userIds.Contains(User.Id);

  public override string ToString()
  {
    var userInfo = User is not null ? $"{User.Name}:{User.Id}" : "<None>";
    return $"{userInfo}@{Id} [{Stage.Status}, {Status}, {nameof(Images)}={Images.Count}]";
  }

  public string Id { get; private set; }

  public string? PublicDnsName
  {
    get => _publicDnsName;
    set
    {
      DockerRemoteApiUrl = !string.IsNullOrEmpty(value)
        ? new Uri("http://" + value + ExpectedDockerApiPort)
        : null;

      _publicDnsName = value;
    }
  }

  public Instant CreatedAt { get; protected set; }
  public UserInfo? User { get; private set; }

  public Uri? DockerRemoteApiUrl { get; private set; }

  public bool IsReady { get; private set; }
  public Instant LastChecked { get; private set; }
  public abstract string ProviderName { get; }


  public NodeStage Stage => _stage.Value;

  public NodeStatus Status => _status.Value;

  // public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

  public IReadOnlyList<RuntimeImageInfo> Images => _images.AsReadOnly();

  public bool IsAlive => Status is NodeStatus.Unknown or NodeStatus.Pending or NodeStatus.Running;

  public bool IsEmptyPendingTermination =>
    Status is NodeStatus.Running or NodeStatus.Pending
    && Images.Count == 0
    && Stage.Status is LifetimeStatus.PendingTermination;
}