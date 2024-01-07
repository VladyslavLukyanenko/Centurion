namespace Centurion.CloudManager.Infra.Services.Aws;

public class AwsConfig
{
  public string AmiId { get; init; } = null!;
  public string InstanceType { get; init; } = null!;
  public string AccessKeyId { get; init; } = null!;
  public string SecretAccessKey { get; init; } = null!;
  public string PlacementRegion { get; init; } = null!;
  public string KeyName { get; init; } = null!;
  public ISet<string> SecurityGroupIds { get; init; } = new HashSet<string>();
}