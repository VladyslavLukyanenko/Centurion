namespace Centurion.Cli.Core.Services.Harvesters;

public interface IHarvesterRegistry
{
  IDisposable Register(IHarvester harvester);
  IHarvester? Get(Guid id);
  IHarvesterRegistryIterator GetIterator();
}

public interface IHarvesterRegistryIterator
{
  Guid? GetNextHarvesterId();
}