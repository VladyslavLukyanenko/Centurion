namespace Centurion.TaskManager.Core.Events;

public interface IEventRepository
{
  ValueTask<ProductCheckedOutEvent> CreateAsync(ProductCheckedOutEvent evt, CancellationToken ct = default);
}