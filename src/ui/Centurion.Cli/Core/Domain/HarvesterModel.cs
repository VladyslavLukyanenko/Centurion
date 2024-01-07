using Centurion.Contracts;
using Centurion.SeedWork.Primitives;

namespace Centurion.Cli.Core.Domain;

public class HarvesterModel : Entity
{
  public HarvesterModel()
    : base(Guid.NewGuid())
  {
  }

  public Guid AccountId { get; set; }
  public Module Module { get; set; }
  public Guid ProxyId { get; set; }
}