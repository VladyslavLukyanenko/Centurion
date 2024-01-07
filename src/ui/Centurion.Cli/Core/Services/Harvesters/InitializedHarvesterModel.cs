using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Domain.Accounts;

namespace Centurion.Cli.Core.Services.Harvesters;

public class InitializedHarvesterModel
{
  public InitializedHarvesterModel(Proxy proxy, Account account, HarvesterModel harvester)
  {
    Proxy = proxy;
    Account = account;
    Harvester = harvester;
  }

  public Proxy Proxy { get; }
  public Account Account { get; }
  public HarvesterModel Harvester { get; }
}