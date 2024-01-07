using Amazon.EC2.Model;
using Centurion.CloudManager.Domain;
using NodaTime.Extensions;

namespace Centurion.CloudManager.Infra.Services.Aws;

public class AwsNode : Node
{
  public AwsNode(Instance src)
    : base(src.InstanceId)
  {
    UpdateFromSource(src);
  }

  public string AvailabilityZone { get; private set; } = null!;
  public string KeyName { get; private set; } = null!;
  public string InstanceType { get; private set; } = null!;

  internal void UpdateFromSource(Instance src)
  {
    AvailabilityZone = src.Placement.AvailabilityZone;
    KeyName = src.KeyName;
    InstanceType = src.InstanceType;
    PublicDnsName = src.PublicDnsName;
    CreatedAt = src.LaunchTime.ToUniversalTime().ToInstant();
    UpdateStatus(src.State.Name.Value.ToAwsNodeStatus());
  }

  public override string ProviderName => Clouds.AWS;
}