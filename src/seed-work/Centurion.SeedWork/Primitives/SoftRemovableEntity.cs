namespace Centurion.SeedWork.Primitives;

public abstract class SoftRemovableEntity : SoftRemovableEntity<long>
{
  protected SoftRemovableEntity()
  {
  }

  protected SoftRemovableEntity(long id)
    : base(id)
  {
  }
}