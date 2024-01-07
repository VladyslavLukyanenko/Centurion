using Centurion.Cli.Core.Composition;
using Centurion.Cli.Core.ViewModels;
using DynamicData;
using NodaTime;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.Domain.Tasks;

[TransientViewModel]
public class CheckoutTaskGroupModel : ViewModelBase
{
  public Guid Id { get; set; }
  public Instant UpdatedAt { get; init; }
  public Instant CreatedAt { get; init; }

  [Reactive] public string Name { get; set; } = null!;

  public ISourceCache<CheckoutTaskModel, Guid> Tasks { get; } = new SourceCache<CheckoutTaskModel, Guid>(_ => _.Id);
}