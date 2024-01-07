using System.Diagnostics.CodeAnalysis;

namespace Centurion.TaskManager.Core;

public class CloudServiceConfig
{
  public bool UseSingleNode { get; init; }

  [MemberNotNullWhen(true, nameof(UseSingleNode))]
  public string? CheckoutUrl { get; init; }
}