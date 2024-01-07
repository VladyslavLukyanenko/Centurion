namespace Centurion.SeedWork.Primitives;

public interface IAuthorAuditable<out T>
{
  T? CreatedBy { get; }
  T? UpdatedBy { get; }
}