using System.Collections.Immutable;
using Centurion.Contracts;
using Centurion.TaskManager.Core;
using CSharpFunctionalExtensions;

namespace Centurion.TaskManager.Application.Services;

public class TaskActivation : ITaskActivation
{
  private readonly IDictionary<Guid, ProxyPoolData> _proxies;
  private readonly IDictionary<Guid, ISet<ProfileData>> _profiles;

  public TaskActivation(Guid taskId, IDictionary<Guid, ProxyPoolData> proxies,
    IDictionary<Guid, ISet<ProfileData>> profiles)
  {
    TaskId = taskId;
    _proxies = proxies;
    _profiles = profiles;
  }

  public Result<TaskActivationDetails> CreateActivated(MappedTask mappedTask)
  {
    var t = mappedTask.Task;
    var profiles = _profiles.GetOrDefault(t.Id, ImmutableHashSet<ProfileData>.Empty)!;
    var checkoutProxy = _proxies.GetOrDefault(t.Id);
    var monitorProxy = _proxies.GetOrDefault(t.Id);
    if (profiles.Count == 0)
    {
      return Result.Failure<TaskActivationDetails>("No profiles found for task " + t.Id);
    }

    var activatedTask = new ActivatedTask(t.Id, t.UserId, t.ProductSku, t.Module, profiles, checkoutProxy, monitorProxy);
    return new TaskActivationDetails(mappedTask, activatedTask);
  }

  public Guid TaskId { get; }
}