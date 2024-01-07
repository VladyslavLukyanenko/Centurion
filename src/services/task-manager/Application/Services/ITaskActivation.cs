using CSharpFunctionalExtensions;

namespace Centurion.TaskManager.Application.Services;

public interface ITaskActivation
{
  Result<TaskActivationDetails> CreateActivated(MappedTask mappedTask);
  Guid TaskId { get; }
}