using Centurion.Cli.Core.Domain.Tasks;
using Centurion.Cli.Core.ViewModels.Tasks;
using CSharpFunctionalExtensions;

namespace Centurion.Cli.Core.Services.Tasks;

public interface ITaskViewService : IExecutionStatusProvider
{
  ValueTask<Result> Save(Guid groupId, IEnumerable<CheckoutTaskModel> tasks, CancellationToken ct = default);
  ValueTask<Result> StartTask(IEnumerable<TaskRowViewModel> tasks, CancellationToken ct = default);
  ValueTask StopTask(IEnumerable<TaskRowViewModel> tasks, CancellationToken ct = default);
  ValueTask<Result> Create(Guid groupId, CheckoutTaskModel proto, int qty, CancellationToken ct = default);
  ValueTask<Result> Duplicate(Guid groupId, IEnumerable<CheckoutTaskModel> tasks, CancellationToken ct = default);
}