namespace Centurion.Cli.Core.Domain.Fields;

public interface ISiblingsDependentField
{
  void Consume(IEnumerable<Field> siblings);
}