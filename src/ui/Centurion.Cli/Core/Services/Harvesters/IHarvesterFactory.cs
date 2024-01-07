namespace Centurion.Cli.Core.Services.Harvesters;

public interface IHarvesterFactory
{
  IHarvester Create();
}