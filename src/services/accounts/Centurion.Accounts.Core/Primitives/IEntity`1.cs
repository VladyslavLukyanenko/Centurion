namespace Centurion.Accounts.Core.Primitives;

public interface IEntity<out TKey>
{
  TKey Id { get; }
}