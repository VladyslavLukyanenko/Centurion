namespace Centurion.SeedWork.Primitives;

public abstract class AuditableEntity
  : AuditableEntity<Guid>
{
  protected AuditableEntity()
  {
  }

  protected AuditableEntity(Guid id)
    : base(id)
  {
  }
}