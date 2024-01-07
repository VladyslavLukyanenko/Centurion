using Centurion.Cli.Core.ViewModels;
using Centurion.SeedWork.Primitives;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.Domain;

public class GeneralSettings : ViewModelBase, IEntity<Guid>
{
  public Guid Id { get; init; }

  [Reactive] public string? CheckoutSoundMp3FilePath { get; set; }
  [Reactive] public string? DeclineSoundMp3FilePath { get; set; }
  [Reactive] public bool CheckoutSoundEnabled { get; init; }
  [Reactive] public bool DeclineSoundEnabled { get; init; }
}