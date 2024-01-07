namespace Centurion.SeedWork.Primitives;

public interface IEntity<out TKey>
{
  TKey Id { get; }
}