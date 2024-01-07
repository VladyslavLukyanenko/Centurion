using Centurion.Cli.Core.ViewModels;
using Centurion.Contracts;
using Centurion.SeedWork.Primitives;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.Domain;

public class SessionModel : ViewModelBase, IEntity<Guid>
{
  public Guid Id { get; init; }

  [Reactive] public Module Module { get; set; }
  [Reactive] public string Name { get; set; } = null!;
  [Reactive] public Guid AccountId { get; set; }
  [Reactive] public SessionStatus Status { get; set; }

  [Reactive] public ISet<string> Cookies { get; set; } = new HashSet<string>();
  [Reactive] public IDictionary<string, string> Extra { get; init; } = new Dictionary<string, string>();
}