using Centurion.CloudManager.Domain;
using Google.Apis.Compute.v1.Data;
using NodaTime;
using NodaTime.Extensions;

namespace Centurion.CloudManager.Infra.Services.Gcp;

public class GcpNode : Node
{
  public GcpNode(Instance src) : base(src.Name) => UpdateFromSource(src);

  public string Zone { get; private set; } = null!;
  public string InstanceType { get; private set; } = null!;
  public Instant LaunchTime { get; private set; }
  public override string ProviderName => Clouds.GCP;
  public IReadOnlyCollection<string> Tags { get; private set; } = Array.Empty<string>();

  internal void UpdateFromSource(Instance src)
  {
    Zone = src.Zone;
    InstanceType = src.MachineType;
    LaunchTime = DateTimeOffset.Parse(src.CreationTimestamp).ToInstant();
    PublicDnsName = src.NetworkInterfaces.SelectMany(_ => _.AccessConfigs)
      .Select(_ => _.NatIP)
      .FirstOrDefault();
    UpdateStatus(src.Status.ToGcpNodeStatus());

    Tags = src.Tags.Items.ToArray();
  }
}