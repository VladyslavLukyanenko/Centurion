namespace Centurion.Accounts.Core.Primitives;

public interface IAuthorAuditable
{
  long? UpdatedBy { get; }
  long? CreatedBy { get; }
}